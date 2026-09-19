namespace StudyPlanner.Infrastructure.Embeddings;

public class OnnxEmbeddingGeneratorOptions
{
    public string ModelPath { get; set; } = "models/all-MiniLM-L6-v2/model.onnx";
    public string VocabPath { get; set; } = "models/all-MiniLM-L6-v2/vocab.txt";
    public int MaxSequenceLength { get; set; } = 256;
}
