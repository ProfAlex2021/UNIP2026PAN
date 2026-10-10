using System.Runtime.ConstrainedExecution;

namespace Pedidos.Validacao;

public sealed class ResultadoValidacao
{
    private ResultadoValidacao(bool aceito, string mensagem)
    {
        Aceito = aceito;
        Mensagem = mensagem;
    }
    public bool Aceito { get; }
    public string Mensagem { get; }
    public static ResultadoValidacao Aceitar(string mensagem) => new(true,
    mensagem);
    public static ResultadoValidacao Rejeitar(string mensagem) => new(false,
    mensagem);
}
internal static class MensagensValidacao
{
    public const string ComandoNulo = "O comando não pode ser nulo.";
    public const string ClienteInativo = "O cliente precisa estar ativo.";
    public const string ValorForaDoIntervalo = "O valor total deve ser maior que zero e menor ou igual a 10.000.";
    public const string QuantidadeForaDoIntervalo = "A quantidade de itens deve estar entre 1 e 100.";
    public const string PedidoCriado = "Comando de criação aceito.";
    public const string PedidoNaoEncontrado = "O pedido a cancelar não foi encontrado.";
    public const string PedidoJaEnviado = "Um pedido já enviado não pode ser cancelado.";
    public const string PedidoCancelado = "Comando de cancelamento aceito.";
    public const string ComandoNaoSuportado = "O tipo de comando não é suportado.";
}
