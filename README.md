# StudyPlanner

Sistema de organização e acompanhamento de estudos para concursos públicos. Backend em .NET (Clean Architecture, CQRS/MediatR, PostgreSQL) e frontend em Angular.

## Pré-requisitos

- .NET SDK 10
- Node.js 22+ (Angular CLI avisa sobre versão em Node < 24.15, mas funciona)
- Docker Desktop
- [yt-dlp](https://github.com/yt-dlp/yt-dlp) no PATH — só necessário para importar material a partir de vídeos do YouTube (`pip install yt-dlp` ou baixar o binário standalone). Sem ele, o resto do sistema funciona normalmente; só a importação de YouTube falha com uma mensagem clara.

## Rodando localmente

```bash
# 1. Sobe Postgres e Redis
docker compose up -d

# 2. Aplica as migrações
cd src/StudyPlanner.Api
dotnet ef database update --project ../StudyPlanner.Infrastructure/StudyPlanner.Infrastructure.csproj --startup-project .

# 3. Sobe a API (http://localhost:5080)
dotnet run

# 4. Em outro terminal, sobe o frontend (http://localhost:4200)
cd web
npm install
npm start
```

## Configurando a chave da Anthropic (extração de edital por IA)

O pipeline de extração de edital (`POST /api/exams/{id}/notice`) usa a Anthropic Messages API para classificar disciplinas e tópicos a partir do PDF. Sem uma chave configurada, o upload e a extração de texto funcionam normalmente, mas o pipeline para de forma controlada nessa etapa (`Notice.Status = Failed`, com mensagem explicando o motivo).

### Onde conseguir a chave

1. Acesse [console.anthropic.com](https://console.anthropic.com) e crie uma conta (ou faça login).
2. No menu, vá em **Settings → API Keys**.
3. Clique em **Create Key**, dê um nome (ex.: `studyplanner-dev`) e copie o valor gerado (começa com `sk-ant-...`). A chave só é exibida uma vez.
4. É necessário ter créditos/billing configurado na conta para as chamadas funcionarem (a Anthropic oferece um saldo inicial gratuito para contas novas).

### Configurando no projeto

A chave **nunca** deve ir para `appsettings.json` nem ser commitada. Use o .NET User Secrets (já habilitado no projeto `StudyPlanner.Api`):

```bash
cd src/StudyPlanner.Api
dotnet user-secrets set "Llm:Anthropic:ApiKey" "sk-ant-sua-chave-aqui"
```

Isso grava a chave em `%APPDATA%\Microsoft\UserSecrets\<id>\secrets.json` (fora do repositório), lida automaticamente pelo ASP.NET Core em ambiente `Development` — basta rodar `dotnet run` normalmente depois.

Alternativa (útil em CI/produção): variável de ambiente `Llm__Anthropic__ApiKey` (dois underscores), lida pela configuração padrão do ASP.NET Core sem nenhuma mudança de código.

Outras opções configuráveis em `Llm:Anthropic` (`appsettings.json`): `Model` (default `claude-sonnet-5`), `BaseUrl`, `MaxTokens`.

## Configurando autenticação (JWT)

A API usa ASP.NET Identity + JWT. Todo endpoint exige um token válido por padrão (política de fallback em `Program.cs`), exceto `POST /api/auth/register` e `POST /api/auth/login`. A chave de assinatura do token **precisa** ser configurada — a API recusa iniciar sem ela (segredo fraco/ausente comprometeria a autenticação inteira):

```bash
cd src/StudyPlanner.Api
dotnet user-secrets set "Jwt:Secret" "$(openssl rand -base64 48)"
```

(qualquer string aleatória com 32+ caracteres serve; o comando acima só é uma forma conveniente de gerar uma). Outras opções em `Jwt` (`appsettings.json`): `Issuer`, `Audience`, `ExpiryMinutes` (default 7 dias).

No frontend, o token fica no `localStorage` (`AuthService`) e é anexado automaticamente a cada requisição via interceptor; um 401 desloga e redireciona para `/login`.

Todo endpoint que lê ou escreve um recurso ligado a um concurso (exame, disciplina, tópico, material, edital, disponibilidade, plano) confere que o concurso pertence ao usuário do token — `ExamOwnershipExtensions.EnsureExamOwnedByAsync` (Application/Common), usado em cada handler. Recurso de outro usuário sempre responde `404` (não `403`) para não confirmar a existência do recurso a quem não é dono. Validado com dois usuários reais via curl: acesso e escrita cruzados retornam 404, o dono legítimo continua funcionando normalmente.

## Baixando o modelo de embeddings (importação de material do aluno)

O pipeline de importação de material (`POST /api/exams/{id}/materials`) gera embeddings localmente com o modelo **distiluse-base-multilingual-cased-v2** (DistilBERT, 50+ idiomas incluindo português) via ONNX Runtime — sem custo, sem chamada externa. Os arquivos do modelo (~540MB) não vão para o git; baixe uma vez:

```bash
mkdir -p src/StudyPlanner.Api/models/distiluse-base-multilingual-cased-v2
cd src/StudyPlanner.Api/models/distiluse-base-multilingual-cased-v2
curl -L -o model.onnx "https://huggingface.co/sentence-transformers/distiluse-base-multilingual-cased-v2/resolve/main/onnx/model.onnx"
curl -L -o vocab.txt "https://huggingface.co/sentence-transformers/distiluse-base-multilingual-cased-v2/resolve/main/vocab.txt"
```

Os caminhos são configuráveis em `Embeddings:ModelPath`/`Embeddings:VocabPath` (`appsettings.json`) — por padrão apontam para essa pasta. Sem os arquivos, o upload de material falha de forma controlada (`StudyMaterial.Status = Failed`) explicando o que falta.

**Histórico:** a primeira versão usava all-MiniLM-L6-v2 (90MB), majoritariamente treinado em inglês — funcionava bem em inglês mas não vinculava material em português a tópicos em português (score ~0 entre idiomas diferentes). Trocado pro distiluse multilíngue, que é "cased" (preserva maiúsculas/acentos — importante em português) e ~6x maior. `Embeddings:DoLowerCase`/`StripAccents`/`UseTokenTypeIds` (`appsettings.json`) existem justamente pra suportar essa troca de modelo sem mexer em código — ajuste se trocar de modelo de novo.

O threshold de relevância material↔tópico (`ProcessMaterialHandler.RelevanceThreshold = 0.25`) foi recalibrado empiricamente pro modelo atual: a escala absoluta de similaridade mudou com a troca de modelo (não é comparável ao threshold antigo de 0.5 do MiniLM). Num teste real em português, chunks de ~800 caracteres pontuaram ~0.32-0.34 contra o tópico correspondente e ~0.14 contra um tópico não relacionado — 0.25 separa os dois casos com margem. Ajuste se notar falsos positivos/negativos, e recalibre sempre que trocar de modelo (a escala não é portável entre modelos diferentes).

### Importando vídeos do YouTube e links

Além de PDF, `POST /api/exams/{id}/materials/youtube` (legenda do vídeo, via yt-dlp) e `POST /api/exams/{id}/materials/link` (texto legível da página, via HtmlAgilityPack) alimentam o mesmo pipeline de chunking/embeddings — nenhum modelo novo, nenhum custo de IA. `Llm:Provider`/chat continuam fora de escopo: isso é só ingestão de mais fontes de texto, não Q&A.

O acesso direto via HTTP ao YouTube (sem yt-dlp) parou de funcionar durante o desenvolvimento: o YouTube passou a exigir um token de sessão amarrado a um carregamento real de página que uma requisição HTTP simples não fornece. yt-dlp contorna isso por ser mantido ativamente contra esse tipo de bloqueio — mas é scraping não-oficial, então pode quebrar de novo se o YouTube mudar de novo (nesse caso, `dotnet tool update` / `pip install -U yt-dlp` costuma resolver).

## Estrutura

```
src/
  StudyPlanner.Api/             # Controllers, DI, configuração
  StudyPlanner.Application/     # CQRS (Commands/Queries), interfaces
  StudyPlanner.Domain/          # Entidades e serviços de domínio puros (PriorityEngine, MasteryCalculator, NoticeSectionSegmenter)
  StudyPlanner.Infrastructure/  # EF Core, storage local, extração de PDF, cliente LLM
  StudyPlanner.Worker/          # Processamento assíncrono (scaffold, ainda não usado)
tests/
  StudyPlanner.Domain.Tests/    # Testes unitários dos serviços de domínio
web/                            # Angular
```
