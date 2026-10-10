namespace Pedidos.Validacao;
// esta classe mistura estado de domínio e formato externo de propósito.
public sealed class PedidoInicial
{
    public Guid Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public List<ItemPedidoInicial> Itens { get; set; } = [];
    public decimal ValorTotal { get; set; }
    public string Situacao { get; set; } = string.Empty;
    // Uma informação operacional pública passa a integrar o contrato acidentalmente.
    public string ObservacaoInterna { get; set; } = string.Empty;
}
public sealed class ItemPedidoInicial
{
    public string Produto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
}
