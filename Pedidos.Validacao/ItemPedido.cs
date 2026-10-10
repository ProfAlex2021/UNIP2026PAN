using System.Runtime.ConstrainedExecution;

namespace Pedidos.Validacao;
// o item é um valor: componentes iguais representam o mesmo item lógico.
public sealed record ItemPedido
{
    public ItemPedido(string produto, int quantidade, decimal precoUnitario)
    {
        if (string.IsNullOrWhiteSpace(produto))
        {
            throw new ArgumentException("O produto deve ser informado.", nameof(produto));
        }
        if (quantidade <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantidade),
                "A quantidade deve ser maior que zero.");
        }
        if (precoUnitario <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(precoUnitario),
                "O preço unitário deve ser maior que zero.");
        }
    }
    public string Produto { get; }
    public int Quantidade { get; }
    public decimal PrecoUnitario { get; }
    public decimal Subtotal => Quantidade * PrecoUnitario;

// A desconstrução mantém explícita a ordem dos três componentes do valor.

    public void Deconstruct(out string produto, out int quantidade, out decimal precoUnitario)
    {
        produto = Produto;
        quantidade = Quantidade;
        precoUnitario = PrecoUnitario;
    }
}
