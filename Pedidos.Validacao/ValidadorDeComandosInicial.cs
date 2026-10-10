namespace Pedidos.Validacao;
// Versão inicial funcional, mantida para tornar visível o custo das ramificações.
public sealed class ValidadorDeComandosInicial
{
    public ResultadoValidacao Validar(object? comando)
    {
        // Interrompe a validação quando nenhum comando foi fornecido.
        // Essa verificação deve ocorrer antes de qualquer acesso ao objeto.
        if (comando is null)
        {
            return ResultadoValidacao.Rejeitar(MensagensValidacao.ComandoNulo);
        }
        // Verifica se o objeto recebido é exatamente um comando CriarPedido.
        // Os comandos são selados; por isso, a comparação exata de Type
        // não exclui possíveis classes derivadas neste exemplo.
        if (comando.GetType() == typeof(CriarPedido))
        {
            var criarPedido = (CriarPedido)comando;
            // Um pedido somente pode ser criado para um cliente ativo.
            // Se o cliente estiver inativo, as demais verificações
            // desse comando deixam de ser necessárias.
            if (!criarPedido.ClienteAtivo)
            {
                return ResultadoValidacao.Rejeitar(MensagensValidacao.ClienteInativo);
            }
            // Rejeita valores que não sejam positivos ou que ultrapassem
            // o limite máximo de R$ 10.000,00 definido pela regra do exemplo.
            if (criarPedido.ValorTotal <= 0m || criarPedido.ValorTotal > 10_000m)
            {
                return ResultadoValidacao.Rejeitar(MensagensValidacao.ValorForaDoIntervalo);
            }
            // Exige que o pedido contenha entre 1 e 100 itens.
            // A condição reúne os dois limites inválidos do intervalo.
            if (criarPedido.QuantidadeItens < 1 || criarPedido.QuantidadeItens > 100)
            {
                return ResultadoValidacao.Rejeitar(MensagensValidacao.QuantidadeForaDoIntervalo);
            }
            // O comando é aceito somente depois de atender
            // a todas as regras específicas de criação do pedido.
            return ResultadoValidacao.Aceitar(
                            MensagensValidacao.PedidoCriado);
        }
        // Verifica se o objeto recebido é exatamente um comando CancelarPedido.
        if (comando.GetType() == typeof(CancelarPedido))
        {
            var cancelarPedido = (CancelarPedido)comando;
            // O cancelamento depende da existência prévia do pedido.
            // Sem um pedido correspondente, não há operação a executar.
            if (!cancelarPedido.PedidoEncontrado)
            {
                return ResultadoValidacao.Rejeitar(
                                    MensagensValidacao.PedidoNaoEncontrado);
            }
            // Impede o cancelamento de um pedido que já foi enviado,
            // pois ele ultrapassou o estado permitido para essa operação.
            if (cancelarPedido.JaEnviado)
            {
                return ResultadoValidacao.Rejeitar(
                                    MensagensValidacao.PedidoJaEnviado);
            }
            // O cancelamento é aceito quando o pedido existe
            // e ainda não foi enviado.
            return ResultadoValidacao.Aceitar(
                            MensagensValidacao.PedidoCancelado);
        }
        // Alcança este ponto quando o objeto não corresponde
        // a nenhum dos tipos de comando reconhecidos pelo validador.
        return ResultadoValidacao.Rejeitar(MensagensValidacao.ComandoNaoSuportado);
    }
}
