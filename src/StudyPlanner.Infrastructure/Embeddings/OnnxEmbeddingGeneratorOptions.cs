namespace StudyPlanner.Infrastructure.Embeddings;

public class OnnxEmbeddingGeneratorOptions
{
    public string ModelPath { get; set; } = "models/distiluse-base-multilingual-cased-v2/model.onnx";
    public string VocabPath { get; set; } = "models/distiluse-base-multilingual-cased-v2/vocab.txt";
    public int MaxSequenceLength { get; set; } = 256;

    /// <summary>Falso para modelos "cased" — maiúsculas/acentos carregam significado em português.</summary>
    public bool DoLowerCase { get; set; } = false;

    /// <summary>Falso para modelos "cased".</summary>
    public bool StripAccents { get; set; } = false;

    /// <summary>DistilBERT (usado no modelo multilíngue atual) não tem esse input; BERT "puro" (ex.: all-MiniLM) tem.</summary>
    public bool UseTokenTypeIds { get; set; } = false;
}
