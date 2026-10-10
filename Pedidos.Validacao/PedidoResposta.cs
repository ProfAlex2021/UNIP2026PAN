namespace Pedidos.Validacao;
// init permite montar o DTO sem torná‑lo uma entidade editável depois da inicialização.
public sealed record PedidoResposta
{
    public Guid Id { get; init; }
    public string Cliente { get; init; } = string.Empty;
    public decimal ValorTotal { get; init; }
    public string Situacao { get; init; } = string.Empty;
    // O contrato expõe uma visão somente leitura criada pelo mapeador, nunca a coleção da entidade.
    public IReadOnlyList<ItemPedidoResposta> Itens { get; init; } = Array.Empty<ItemPedidoResposta>();
}
public sealed record ItemPedidoResposta(
    string Produto,
    int Quantidade,
    decimal Subtotal);
