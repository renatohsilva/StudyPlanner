using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using StudyPlanner.Application.Common.Interfaces;

namespace StudyPlanner.Infrastructure.Embeddings;

/// <summary>
/// Embeddings semânticos locais via all-MiniLM-L6-v2 (ONNX Runtime) — zero custo, zero chamada
/// externa. Tokenização WordPiece implementada à mão (BertWordPieceTokenizer); pooling por média
/// ponderada pela attention mask + normalização L2, como o modelo foi treinado.
/// </summary>
public sealed class OnnxEmbeddingGenerator : IEmbeddingGenerator, IDisposable
{
    private readonly OnnxEmbeddingGeneratorOptions _options;
    private readonly Lazy<InferenceSession> _session;
    private readonly Lazy<BertWordPieceTokenizer> _tokenizer;

    public OnnxEmbeddingGenerator(IOptions<OnnxEmbeddingGeneratorOptions> options)
    {
        _options = options.Value;

        _session = new Lazy<InferenceSession>(() =>
        {
            if (!File.Exists(_options.ModelPath))
            {
                throw new InvalidOperationException(
                    $"Modelo de embeddings não encontrado em '{_options.ModelPath}'. Baixe o all-MiniLM-L6-v2 " +
                    "em formato ONNX (ver README) antes de importar materiais.");
            }

            return new InferenceSession(_options.ModelPath);
        });

        _tokenizer = new Lazy<BertWordPieceTokenizer>(() =>
        {
            if (!File.Exists(_options.VocabPath))
            {
                throw new InvalidOperationException($"Vocabulário do tokenizador não encontrado em '{_options.VocabPath}'.");
            }

            return new BertWordPieceTokenizer(_options.VocabPath);
        });
    }

    public Task<float[]> GenerateAsync(string text, CancellationToken cancellationToken)
    {
        var (inputIds, attentionMask, tokenTypeIds) = _tokenizer.Value.Tokenize(text, _options.MaxSequenceLength);
        var seqLen = inputIds.Length;

        var inputIdsTensor = new DenseTensor<long>(inputIds, [1, seqLen]);
        var attentionMaskTensor = new DenseTensor<long>(attentionMask, [1, seqLen]);
        var tokenTypeIdsTensor = new DenseTensor<long>(tokenTypeIds, [1, seqLen]);

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdsTensor)
        };

        using var results = _session.Value.Run(inputs);
        var lastHiddenState = results.First(r => r.Name == "last_hidden_state").AsTensor<float>();

        var hiddenSize = lastHiddenState.Dimensions[2];
        var pooled = new float[hiddenSize];
        var validTokenCount = 0;

        for (var t = 0; t < seqLen; t++)
        {
            if (attentionMask[t] == 0) continue;
            validTokenCount++;

            for (var h = 0; h < hiddenSize; h++)
            {
                pooled[h] += lastHiddenState[0, t, h];
            }
        }

        if (validTokenCount > 0)
        {
            for (var h = 0; h < hiddenSize; h++)
            {
                pooled[h] /= validTokenCount;
            }
        }

        NormalizeInPlace(pooled);

        return Task.FromResult(pooled);
    }

    private static void NormalizeInPlace(float[] vector)
    {
        var norm = MathF.Sqrt(vector.Sum(v => v * v));
        if (norm == 0f) return;

        for (var i = 0; i < vector.Length; i++)
        {
            vector[i] /= norm;
        }
    }

    public void Dispose()
    {
        if (_session.IsValueCreated) _session.Value.Dispose();
    }
}
