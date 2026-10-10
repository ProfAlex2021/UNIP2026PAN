namespace Pedidos.Validacao;
// As regras são equivalentes às da versão inicial, agora expressas
// por meio de uma expressão switch e de padrões de propriedades.
public sealed class ValidadorDeComandos
{
    public ResultadoValidacao Validar(object? comando) => comando switch
    {
        // Trata explicitamente a ausência de um comando.
        // Como esta regra aparece primeiro, nenhum outro padrão
        // será avaliado quando o argumento recebido for nulo.
        null => ResultadoValidacao.Rejeitar(MensagensValidacao.ComandoNulo),
        // Reconhece um objeto do tipo CriarPedido cuja propriedade
        // ClienteAtivo possui o valor false.
        //
        // A primeira regra aplicável encerra a expressão switch.
        // Por isso, a ordem preserva a mensagem mais específica
        // quando o comando apresenta mais de uma condição inválida.
        CriarPedido { ClienteAtivo: false } =>
            ResultadoValidacao.Rejeitar(MensagensValidacao.ClienteInativo),
        // Reconhece um comando CriarPedido cujo valor total está
        // fora do intervalo permitido.
        //
        // O operador “or” reúne duas possibilidades:
        // valor menor ou igual a zero, ou valor superior a 10.000.
        CriarPedido { ValorTotal: <= 0m or > 10_000m } =>
            ResultadoValidacao.Rejeitar(MensagensValidacao.ValorForaDoIntervalo),
        // Rejeita pedidos cuja quantidade de itens seja inferior
        // a 1 ou superior a 100.
        //
        // Os padrões relacionais são aplicados diretamente
        // à propriedade QuantidadeItens.
        CriarPedido { QuantidadeItens: < 1 or > 100 } =>
            ResultadoValidacao.Rejeitar(MensagensValidacao.QuantidadeForaDoIntervalo),
        // Aceita somente o comando CriarPedido que satisfaz
        // simultaneamente todas as regras de criação.
        //
        // O cliente deve estar ativo, o valor total deve permanecer
        // entre 0 e 10.000, e a quantidade deve ficar entre 1 e 100.
        //
        // O operador “and” combina os limites inferior e superior
        // aplicados a uma mesma propriedade.
        CriarPedido
        {
            ClienteAtivo: true,
            ValorTotal: > 0m and <= 10_000m,
            QuantidadeItens: >= 1 and <= 100
        } => ResultadoValidacao.Aceitar(MensagensValidacao.PedidoCriado),
        // Reconhece um comando CancelarPedido para o qual
        // nenhum pedido correspondente foi encontrado.
        CancelarPedido { PedidoEncontrado: false } =>
            ResultadoValidacao.Rejeitar(MensagensValidacao.PedidoNaoEncontrado),
        // Rejeita o cancelamento quando o pedido existe,
        // mas já foi enviado.
        //
        // As duas propriedades precisam corresponder aos valores
        // declarados no padrão para que esta regra seja aplicada.
        CancelarPedido
        {
            PedidoEncontrado: true,
            JaEnviado: true
        } => ResultadoValidacao.Rejeitar(MensagensValidacao.PedidoJaEnviado),
        // Aceita o cancelamento quando o pedido foi encontrado
        // e não possui o valor true na propriedade JaEnviado.
        //
        // Neste caso, “not true” equivale a false porque JaEnviado
        // é uma propriedade booleana não anulável.
        CancelarPedido
        {
            PedidoEncontrado: true,
            JaEnviado: not true
        } => ResultadoValidacao.Aceitar(MensagensValidacao.PedidoCancelado),
        // O descarte “_” representa qualquer valor que não tenha
        // correspondido aos padrões anteriores.
        //
        // Como o parâmetro possui o tipo object, podem ser recebidos
        // objetos que não sejam CriarPedido nem CancelarPedido.
        // Esta regra torna o tratamento desses casos explícito.
        _ => ResultadoValidacao.Rejeitar(MensagensValidacao.ComandoNaoSuportado)
    };
}
