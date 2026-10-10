using System.Collections.ObjectModel;

namespace Pedidos.Validacao;

public enum SituacaoPedido
{
    Aberto,
    Enviado
}

// Pedido permanece classe porque sua identidade atravessa mudanças de estado.
public sealed class Pedido
{
    private readonly List<ItemPedido> _itens = [];
    private readonly ReadOnlyCollection<ItemPedido> _itensSomenteLeitura;
    public Pedido(Guid id, string cliente)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("O identificador do pedido deve ser informado.", nameof(id));
        }
        
        if (string.IsNullOrWhiteSpace(cliente))
        {
            throw new ArgumentException("O cliente deve ser informado.",
            nameof(cliente));
        }
        Id = id;
        Cliente = cliente;
        Situacao = SituacaoPedido.Aberto;
        _itensSomenteLeitura = _itens.AsReadOnly();
    }
    public Guid Id { get; }
    public string Cliente { get; }
    public SituacaoPedido Situacao { get; private set; }
    public IReadOnlyList<ItemPedido> Itens => _itensSomenteLeitura;
    public decimal ValorTotal => _itens.Sum(item => item.Subtotal);
    // Esta propriedade pertence ao domínio, mas não faz parte de PedidoResposta.
    public string CodigoDeRoteamentoInterno { get; private set; } = string.Empty;
    public void AdicionarItem(ItemPedido item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (Situacao != SituacaoPedido.Aberto)
        {
            throw new InvalidOperationException("Um pedido enviado não aceita novos itens.");
        }
        _itens.Add(item);
    }
    public void DefinirCodigoDeRoteamentoInterno(string codigo)
    {
        CodigoDeRoteamentoInterno = codigo ?? string.Empty;
    }
    public void MarcarComoEnviado()
    {
        if (_itens.Count == 0)
        {
            throw new InvalidOperationException("Um pedido sem itens não pode ser enviado.");
        }
        Situacao = SituacaoPedido.Enviado;
    }
}
