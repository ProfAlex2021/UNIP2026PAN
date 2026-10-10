using System.Collections.ObjectModel;
namespace Pedidos.Validacao;

// cada direção do mapeamento declara quais dados atravessam a fronteira.
public static class MapeadorDePedido
{
    public static Pedido ParaEntidade(CriarPedidoRequisicao requisicao, Guid idGerado)
    {
        ArgumentNullException.ThrowIfNull(requisicao);
        if (requisicao.Itens is null || requisicao.Itens.Count == 0)
        {
            throw new InvalidOperationException("A requisição precisa conter ao menos um item.");
        }
        // Identidade, situação e total não vêm da requisição; permanecem sob controle do domínio.
        var pedido = new Pedido(idGerado, requisicao.Cliente);

        foreach (var item in requisicao.Itens)
        {
            pedido.AdicionarItem(new ItemPedido(item.Produto, item.Quantidade, item.PrecoUnitario));
        }
        return pedido;
    }
    public static PedidoResposta ParaResposta(Pedido pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);

        var itens = pedido.Itens
            .Select(item => new ItemPedidoResposta(item.Produto, item.Quantidade, item.Subtotal))
            .ToArray();

        // A cópia impede que a resposta ofereça acesso à lista interna mutável da entidade.
        ReadOnlyCollection<ItemPedidoResposta> itensSomenteLeitura = Array.AsReadOnly(itens);

        return new PedidoResposta
        {
            Id = pedido.Id,
            Cliente = pedido.Cliente,
            ValorTotal = pedido.ValorTotal,
            Situacao = pedido.Situacao.ToString(),
            Itens = itensSomenteLeitura
        };
    }
}
