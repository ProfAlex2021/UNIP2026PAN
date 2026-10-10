namespace Pedidos.Validacao.Testes;

[TestClass]
public sealed class MapeadorDePedidoTestes
{
    [TestMethod]
    public void Mapeamento_de_entrada_preserva_no_dominio_identidade_situacao_e_total()
    {
        var idGerado = Guid.Parse("5e08ec38‑4c86‑4fb5‑9263‑af4dc2322d64");
        var requisicao = new CriarPedidoRequisicao(
            "Cliente A",
            [
                new ItemPedidoRequisicao("Teclado", 2, 150m),
                new ItemPedidoRequisicao("Mouse", 1, 80m)
            ]);
        var pedido = MapeadorDePedido.ParaEntidade(requisicao, idGerado);

        Assert.AreEqual(idGerado, pedido.Id);
        Assert.AreEqual("Cliente A", pedido.Cliente);
        Assert.AreEqual(SituacaoPedido.Aberto, pedido.Situacao);
        Assert.AreEqual(380m, pedido.ValorTotal);
        Assert.HasCount(2, pedido.Itens);
    }

    [TestMethod]
    public void Mapeamento_de_saida_cria_representacao_independente_da_colecao_da_entidade()
    {
        var pedido = CriarPedidoDoDominio();
        var resposta = MapeadorDePedido.ParaResposta(pedido);

        pedido.AdicionarItem(new ItemPedido("Cabo", 1, 25m));

        Assert.HasCount(1, resposta.Itens);
        Assert.HasCount(2, pedido.Itens);
        Assert.AreEqual(300m, resposta.ValorTotal);
        Assert.AreEqual(325m, pedido.ValorTotal);
    }
    [TestMethod]
    public void Entidade_preserva_a_identidade_durante_mudancas_controladas()
    {
        var pedido = CriarPedidoDoDominio();
        var identidadeOriginal = pedido.Id;

        pedido.DefinirCodigoDeRoteamentoInterno("ROTA‑EXPRESSA");
        pedido.MarcarComoEnviado();

        Assert.AreEqual(identidadeOriginal, pedido.Id);
        Assert.AreEqual(SituacaoPedido.Enviado, pedido.Situacao);
        Assert.AreEqual("ROTA‑EXPRESSA", pedido.CodigoDeRoteamentoInterno);
    }

    [TestMethod]
    public void Item_de_pedido_usa_igualdade_por_valor_e_desconstrucao()
    {
        var primeiro = new ItemPedido("Teclado", 2, 150m);
        var segundo = new ItemPedido("Teclado", 2, 150m);

        var (produto, quantidade, precoUnitario) = primeiro;

        Assert.AreEqual(primeiro, segundo);
        Assert.AreNotSame(primeiro, segundo);
        Assert.AreEqual("Teclado", produto);
        Assert.AreEqual(2, quantidade);
        Assert.AreEqual(150m, precoUnitario);
    }
    private static Pedido CriarPedidoDoDominio()
    {
        var pedido = new Pedido(
            Guid.Parse("43da9b57‑986b‑4e50‑a0f7‑f7682a0a920d"),
            "Cliente A");

        pedido.AdicionarItem(new ItemPedido("Teclado", 2, 150m));

        return pedido;
    }
}
