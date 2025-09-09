/**
 * Esquema de tipos TypeScript para MCP-BOE Server
 * Define todas las interfaces y tipos para las herramientas del servidor MCP-BOE
 */

// ================================
// TIPOS BASE
// ================================

export type BoeSection = "I" | "II" | "III" | "IV" | "V";
export type DocumentFormat = "xml" | "pdf" | "html";
export type AnalysisType = "summary" | "impact" | "changes" | "comparison";

export interface DateRange {
  from: string; // YYYY-MM-DD
  to: string;   // YYYY-MM-DD
}

// ================================
// HERRAMIENTAS DE LEGISLACIÓN
// ================================

export interface BoeLegislationSummaryParams {
  date?: string; // YYYY-MM-DD
  limit?: number; // 1-100, default: 20
}

export interface BoeLegislationSearchParams {
  query?: string;
  date?: string; // YYYY-MM-DD
  dateRange?: DateRange;
  department?: string;
  section?: BoeSection;
  documentType?: string;
  limit?: number; // 1-100, default: 20
}

export interface BoeLegislationDocumentParams {
  id: string; // BOE-A-YYYY-NNNNN
  format?: DocumentFormat; // default: "xml"
  includeMetadata?: boolean; // default: true
}

// ================================
// HERRAMIENTAS DE SUMARIOS
// ================================

export interface BoeSummaryDailyParams {
  date: string; // YYYY-MM-DD
  section?: BoeSection | "all"; // default: "all"
  includeContent?: boolean; // default: false
}

export interface BoeSummarySearchParams {
  query: string;
  section?: BoeSection;
  dateRange?: DateRange;
  limit?: number; // 1-100, default: 20
}

export interface BoeSummarySectionParams {
  section: BoeSection;
  date: string; // YYYY-MM-DD
  detailed?: boolean; // default: false
}

// ================================
// HERRAMIENTAS AUXILIARES
// ================================

export interface BoeAuxiliaryDepartmentsParams {
  filter?: string;
  includeStats?: boolean; // default: false
}

export interface BoeAuxiliarySectionsParams {
  includeDescription?: boolean; // default: true
}

export interface BoeAuxiliarySubsectionsParams {
  section?: BoeSection;
}

export interface BoeAuxiliaryDocumentTypesParams {
  section?: BoeSection;
  includeExamples?: boolean; // default: false
}

export interface BoeAuxiliaryTerritoriesParams {
  includeStats?: boolean; // default: false
}

// ================================
// RESPUESTAS DE LA API
// ================================

export interface BoeDocument {
  id: string;
  title: string;
  date: string;
  section: BoeSection;
  department: string;
  documentType: string;
  summary?: string;
  content?: string;
  metadata?: {
    pages: number;
    size: string;
    format: DocumentFormat;
    url: string;
  };
}

export interface BoeSummaryEntry {
  id: string;
  title: string;
  section: BoeSection;
  subsection?: string;
  department: string;
  documentType: string;
  pages: string;
  summary?: string;
}

export interface BoeDepartment {
  code: string;
  name: string;
  fullName: string;
  stats?: {
    totalDocuments: number;
    lastPublication: string;
  };
}

export interface BoeSectionInfo {
  code: BoeSection;
  name: string;
  description?: string;
  documentTypes: string[];
}

export interface BoeDocumentType {
  code: string;
  name: string;
  description: string;
  section: BoeSection;
  examples?: string[];
}

export interface BoeTerritory {
  code: string;
  name: string;
  type: "comunidad_autonoma" | "provincia" | "ciudad_autonoma";
  stats?: {
    publications: number;
    lastUpdate: string;
  };
}

// ================================
// RESPUESTAS DE HERRAMIENTAS
// ================================

export interface LegislationSummaryResponse {
  date?: string;
  totalDocuments: number;
  documentsBySection: Record<BoeSection, number>;
  recentDocuments: BoeDocument[];
  stats: {
    totalPages: number;
    mainDepartments: string[];
    documentTypes: string[];
  };
}

export interface LegislationSearchResponse {
  query?: string;
  totalResults: number;
  documents: BoeDocument[];
  filters: {
    sections: BoeSection[];
    departments: string[];
    documentTypes: string[];
    dateRange?: DateRange;
  };
}

export interface DocumentResponse {
  document: BoeDocument;
  content: string;
  format: DocumentFormat;
  metadata: {
    size: string;
    downloadUrl: string;
    lastModified: string;
  };
}

export interface DailySummaryResponse {
  date: string;
  sections: Record<BoeSection, BoeSummaryEntry[]>;
  totalDocuments: number;
  stats: {
    pagesBySection: Record<BoeSection, number>;
    departmentCount: number;
  };
}

export interface SummarySearchResponse {
  query: string;
  totalResults: number;
  summaries: BoeSummaryEntry[];
  relatedTerms: string[];
}

export interface SectionSummaryResponse {
  section: BoeSection;
  date: string;
  sectionName: string;
  entries: BoeSummaryEntry[];
  totalDocuments: number;
  totalPages: number;
}

// ================================
// CONFIGURACIÓN DEL SERVIDOR
// ================================

export interface McpBoeServerConfig {
  baseUrl: string;
  version: string;
  rateLimits: {
    requestsPerMinute: number;
    requestsPerHour: number;
  };
  endpoints: {
    healthCheck: string;
    documentation: string;
  };
}

// ================================
// HERRAMIENTAS MCP
// ================================

export interface McpTool {
  name: string;
  description: string;
  inputSchema: {
    type: "object";
    properties: Record<string, any>;
    required?: string[];
    additionalProperties: boolean;
  };
}

export type BoeToolName =
  | "boe_legislation_summary"
  | "boe_legislation_search"
  | "boe_legislation_document"
  | "boe_summary_daily"
  | "boe_summary_search"
  | "boe_summary_section"
  | "boe_auxiliary_departments"
  | "boe_auxiliary_sections"
  | "boe_auxiliary_subsections"
  | "boe_auxiliary_document_types"
  | "boe_auxiliary_territories";

// ================================
// UTILIDADES Y VALIDACIONES
// ================================

export class BoeValidators {
  static isValidDate(date: string): boolean {
    const regex = /^\d{4}-\d{2}-\d{2}$/;
    if (!regex.test(date)) return false;
    const dateObj = new Date(date);
    return dateObj instanceof Date && !isNaN(dateObj.getTime());
  }

  static isValidBoeId(id: string): boolean {
    const regex = /^BOE-[A-Z]-\d{4}-\d{5}$/;
    return regex.test(id);
  }

  static isValidSection(section: string): section is BoeSection {
    return ["I", "II", "III", "IV", "V"].includes(section);
  }

  static formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  static validateLimit(limit?: number): number {
    if (!limit) return 20;
    return Math.min(Math.max(limit, 1), 100);
  }
}

// ================================
// EJEMPLOS DE USO
// ================================

export const BoeUsageExamples = {
  // Búsqueda básica de legislación
  basicLegislationSearch: {
    tool: "boe_legislation_search" as BoeToolName,
    params: {
      query: "inteligencia artificial",
      section: "I" as BoeSection,
      limit: 10
    } as BoeLegislationSearchParams
  },

  // Obtener sumario diario
  dailySummary: {
    tool: "boe_summary_daily" as BoeToolName,
    params: {
      date: "2024-01-15",
      section: "all"
    } as BoeSummaryDailyParams
  },

  // Buscar por departamento
  departmentSearch: {
    tool: "boe_legislation_search" as BoeToolName,
    params: {
      department: "Ministerio de Justicia",
      dateRange: {
        from: "2024-01-01",
        to: "2024-01-31"
      }
    } as BoeLegislationSearchParams
  },

  // Obtener documento específico
  getDocument: {
    tool: "boe_legislation_document" as BoeToolName,
    params: {
      id: "BOE-A-2024-00001",
      format: "xml",
      includeMetadata: true
    } as BoeLegislationDocumentParams
  }
};

export default {
  BoeValidators,
  BoeUsageExamples
};
