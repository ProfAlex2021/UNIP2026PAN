//métodos comuns representam a fronteira sem introduzir HTTP ou ASP.NET Core.
namespace Pedidos.Validacao;

public sealed class PedidoEndpointsInicial
{
    public PedidoInicial Criar(PedidoInicial pedidoRecebido, Guid idGerado)
    {
        ArgumentNullException.ThrowIfNull(pedidoRecebido);

        if (pedidoRecebido.Itens.Count == 0)
        {
            throw new InvalidOperationException("O pedido inicial precisa conter ao menos um item.");
        }

        // A mesma instância recebida é alterada e devolvida; não existe fronteira entre entrada, domínio e saída.
        pedidoRecebido.Id = idGerado;
        pedidoRecebido.ValorTotal = pedidoRecebido.Itens.Sum(
            item => item.Quantidade * item.PrecoUnitario);
        pedidoRecebido.Situacao = "Aberto";
        pedidoRecebido.ObservacaoInterna = "Aguardando conferência operacional.";
        
        return pedidoRecebido;
    }
}
