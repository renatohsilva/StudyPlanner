# StudyPlanner

Sistema de organização e acompanhamento de estudos para concursos públicos. Backend em .NET (Clean Architecture, CQRS/MediatR, PostgreSQL) e frontend em Angular.

## Pré-requisitos

- .NET SDK 10
- Node.js 22+ (Angular CLI avisa sobre versão em Node < 24.15, mas funciona)
- Docker Desktop

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

## Baixando o modelo de embeddings (importação de material do aluno)

O pipeline de importação de material (`POST /api/exams/{id}/materials`) gera embeddings localmente com o modelo **all-MiniLM-L6-v2** via ONNX Runtime — sem custo, sem chamada externa. Os arquivos do modelo (~90MB) não vão para o git; baixe uma vez:

```bash
mkdir -p src/StudyPlanner.Api/models/all-MiniLM-L6-v2
cd src/StudyPlanner.Api/models/all-MiniLM-L6-v2
curl -L -o model.onnx "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx"
curl -L -o vocab.txt "https://huggingface.co/sentence-transformers/all-MiniLM-L6-v2/resolve/main/vocab.txt"
```

Os caminhos são configuráveis em `Embeddings:ModelPath`/`Embeddings:VocabPath` (`appsettings.json`) — por padrão apontam para essa pasta. Sem os arquivos, o upload de material falha de forma controlada (`StudyMaterial.Status = Failed`) explicando o que falta.

O threshold de relevância material↔tópico (`ProcessMaterialHandler.RelevanceThreshold = 0.5`) foi calibrado empiricamente: embeddings BERT-family têm similaridade "de base" alta entre frases quaisquer do mesmo idioma, então um corte baixo (ex. 0.35) deixa passar tópicos não relacionados. Ajuste se notar falsos positivos/negativos.

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
