export type SituacaoMobilizacao = 'Aguardando' | 'Liberado' | 'Afastado' | 'Desmobilizado';

export type ImpedimentoLiberacao = {
  codigo: string;
  requisitoCodigo: string;
  motivo: string;
  itemChecklistId?: string | null;
};

export type ResultadoLiberacao = {
  mobilizacaoId: string;
  situacao: SituacaoMobilizacao;
  liberado: boolean;
  avaliadoEm: string;
  impedimentos: ImpedimentoLiberacao[];
};

export const impedimentoIdentidade: ImpedimentoLiberacao = {
  codigo: 'IDENTIDADE_PENDENTE',
  requisitoCodigo: 'DOC_OFICIAL_FOTO',
  motivo: 'Documento de identidade pendente.',
  itemChecklistId: '11111111-1111-1111-1111-111111111111',
};

export const liberacaoAguardando: ResultadoLiberacao = {
  mobilizacaoId: '22222222-2222-2222-2222-222222222222',
  situacao: 'Aguardando',
  liberado: false,
  avaliadoEm: '2026-09-27T12:00:00.000Z',
  impedimentos: [impedimentoIdentidade],
};

export const liberacaoCompleta: ResultadoLiberacao = {
  ...liberacaoAguardando,
  situacao: 'Liberado',
  liberado: true,
  impedimentos: [],
};

export const movimentoEpiEntrega = {
  id: '33333333-3333-3333-3333-333333333333',
  mobilizacaoId: liberacaoAguardando.mobilizacaoId,
  tipo: 'Entrega',
  epi: 'Capacete',
  quantidade: 1,
  numeroCa: '12345',
  data: '2026-09-27',
  orientacaoUso: true,
  responsabilidadeGuarda: true,
  aceiteTrabalhador: true,
  registradoEm: '2026-09-27T12:05:00.000Z',
};

export const integracaoObraValida = {
  id: '44444444-4444-4444-4444-444444444444',
  mobilizacaoId: liberacaoAguardando.mobilizacaoId,
  obraId: '55555555-5555-5555-5555-555555555555',
  dataHora: '2026-09-27T11:00:00-03:00',
  conteudo: 'Integração de segurança na obra.',
  instrutor: 'Instrutor interno',
  aceiteTrabalhador: true,
  refazer: false,
  criadoEm: '2026-09-27T14:00:00.000Z',
};
