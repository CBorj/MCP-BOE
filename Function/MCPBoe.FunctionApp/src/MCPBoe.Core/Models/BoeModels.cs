using System.Text.Json.Serialization;

namespace MCPBoe.Core.Models;

/// <summary>
/// Base model for BOE API responses
/// </summary>
public abstract record BoeBaseModel
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("titulo")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("fecha")]
    public string Date { get; init; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;
}

/// <summary>
/// BOE Legislation model for consolidated legislation
/// </summary>
public record BoeLegislationModel : BoeBaseModel
{
    [JsonPropertyName("tipo_norma")]
    public string NormType { get; init; } = string.Empty;

    [JsonPropertyName("numero")]
    public string Number { get; init; } = string.Empty;

    [JsonPropertyName("departamento")]
    public string Department { get; init; } = string.Empty;

    [JsonPropertyName("rango")]
    public string Range { get; init; } = string.Empty;

    [JsonPropertyName("vigente")]
    public bool IsActive { get; init; }

    [JsonPropertyName("texto")]
    public string Text { get; init; } = string.Empty;

    [JsonPropertyName("estructura")]
    public BoeStructureModel? Structure { get; init; }
}

/// <summary>
/// BOE Summary model for daily summaries
/// </summary>
public record BoeSummaryModel : BoeBaseModel
{
    [JsonPropertyName("seccion")]
    public string Section { get; init; } = string.Empty;

    [JsonPropertyName("emisor")]
    public string Issuer { get; init; } = string.Empty;

    [JsonPropertyName("paginas")]
    public string Pages { get; init; } = string.Empty;

    [JsonPropertyName("tipo_documento")]
    public string DocumentType { get; init; } = string.Empty;
}

/// <summary>
/// BOE Auxiliary data model for departments, ranges, etc.
/// </summary>
public record BoeAuxiliaryModel
{
    [JsonPropertyName("codigo")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("descripcion")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string Type { get; init; } = string.Empty;
}

/// <summary>
/// BOE Structure model for legislation structure
/// </summary>
public record BoeStructureModel
{
    [JsonPropertyName("titulos")]
    public List<BoeTitleModel> Titles { get; init; } = new();

    [JsonPropertyName("capitulos")]
    public List<BoeChapterModel> Chapters { get; init; } = new();

    [JsonPropertyName("articulos")]
    public List<BoeArticleModel> Articles { get; init; } = new();
}

/// <summary>
/// BOE Title model for legislation titles
/// </summary>
public record BoeTitleModel
{
    [JsonPropertyName("numero")]
    public string Number { get; init; } = string.Empty;

    [JsonPropertyName("titulo")]
    public string Title { get; init; } = string.Empty;
}

/// <summary>
/// BOE Chapter model for legislation chapters
/// </summary>
public record BoeChapterModel
{
    [JsonPropertyName("numero")]
    public string Number { get; init; } = string.Empty;

    [JsonPropertyName("titulo")]
    public string Title { get; init; } = string.Empty;
}

/// <summary>
/// BOE Article model for legislation articles
/// </summary>
public record BoeArticleModel
{
    [JsonPropertyName("numero")]
    public string Number { get; init; } = string.Empty;

    [JsonPropertyName("titulo")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("contenido")]
    public string Content { get; init; } = string.Empty;
}
