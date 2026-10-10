namespace Pedidos.Validacao;
// o record posicional declara somente os dados que o consumidor pode fornecer.
public sealed record CriarPedidoRequisicao(
    string Cliente,
    IReadOnlyList<ItemPedidoRequisicao> Itens);

public sealed record ItemPedidoRequisicao(
    string Produto,
    int Quantidade,
    decimal PrecoUnitario);
