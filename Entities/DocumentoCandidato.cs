namespace a2_tp3_job_connect.Entities;

public class DocumentoCandidato : EntidadeBase
{
    public Guid ResumeId { get; set; }
    public DocumentType Type { get; set; } = DocumentType.ResumePdf;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
    public string StoragePath { get; set; } = string.Empty;
    public long SizeInBytes { get; set; }

    public Curriculo Resume { get; set; } = null!;
}
