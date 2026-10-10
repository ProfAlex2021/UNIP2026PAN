using System.Reflection;
namespace Pedidos.Validacao.Testes;

// a inspeção dos membros públicos torna a fronteira verificável sem serialização ou HTTP.
[TestClass]
public sealed class ContratoDePedidoTestes
{
    [TestMethod]
    public void Records_distintos_com_os_mesmos_componentes_sao_iguais_por_valor()
    {
        var primeiro = new ItemPedidoRequisicao("Teclado", 2, 150m);
        var segundo = new ItemPedidoRequisicao("Teclado", 2, 150m);

        Assert.AreEqual(primeiro, segundo);
        Assert.AreNotSame(primeiro, segundo);
    }
    [TestMethod]
    public void Records_com_componentes_diferentes_nao_sao_iguais()
    {
        var primeiro = new ItemPedidoRequisicao("Teclado", 2, 150m);
        var segundo = new ItemPedidoRequisicao("Teclado", 3, 150m);

        Assert.AreNotEqual(primeiro, segundo);
    }
    [TestMethod]
    public void Respostas_com_os_mesmos_componentes_e_a_mesma_referencia_de_itens_sao_iguais()
    {
        var primeira = CriarResposta();
        var segunda = new PedidoResposta
        {
            Id = primeira.Id,
            Cliente = primeira.Cliente,
            ValorTotal = primeira.ValorTotal,
            Situacao = primeira.Situacao,
            Itens = primeira.Itens
        };
        Assert.AreEqual(primeira, segunda);
        Assert.AreNotSame(primeira, segunda);
    }
    [TestMethod]
    public void Respostas_mapeadas_separadamente_nao_recebem_igualdade_profunda_da_colecao()
    {
        var pedido = CriarPedidoDoDominio();
        var primeira = MapeadorDePedido.ParaResposta(pedido);
        var segunda = MapeadorDePedido.ParaResposta(pedido);

        Assert.IsTrue(primeira.Itens.SequenceEqual(segunda.Itens));
        Assert.AreNotSame(primeira.Itens, segunda.Itens);
        Assert.AreNotEqual(primeira, segunda);
    }
    [TestMethod]
    public void Requisicao_posicional_pode_ser_desconstruida()
    {
        IReadOnlyList<ItemPedidoRequisicao> itens =
            [new ItemPedidoRequisicao("Teclado", 2, 150m)];
        var requisicao = new CriarPedidoRequisicao("Cliente A", itens);
        var (cliente, itensDesconstruidos) = requisicao;

        Assert.AreEqual("Cliente A", cliente);
        Assert.AreSame(itens, itensDesconstruidos);
        
    }

    [TestMethod]
    public void With_cria_nova_resposta_preserva_original_e_compartilha_referencia_nao_substituida()
    {
        var respostaOriginal = CriarResposta();
        var respostaEnviada = respostaOriginal with { Situacao = "Enviado" };

        Assert.AreNotSame(respostaOriginal, respostaEnviada);
        Assert.AreEqual("Aberto", respostaOriginal.Situacao);
        Assert.AreEqual("Enviado", respostaEnviada.Situacao);
        Assert.AreSame(respostaOriginal.Itens, respostaEnviada.Itens);
    }
    [TestMethod]
    public void With_compartilha_lista_mutavel_e_por_isso_nao_garante_imutabilidade_profunda()
    {
        var original = new ContratoComListaMutavel("Pedido", ["urgente"]);
        var copia = original with { Nome = "Pedido revisto" };

        original.Marcadores.Add("presente");

        Assert.AreSame(original.Marcadores, copia.Marcadores);
        CollectionAssert.AreEqual(
            new[] { "urgente", "presente" },
            copia.Marcadores);
    }
    [TestMethod]
    public void Colecao_da_resposta_nao_aceita_mutacao_pelo_contrato_exposto()
    {
        var resposta = CriarResposta();
        var listaExposta = (IList<ItemPedidoResposta>)resposta.Itens;

        Assert.ThrowsExactly<NotSupportedException>(
            () => listaExposta.Add(new ItemPedidoResposta("Mouse", 1, 80m)));
    }
    [TestMethod]
    public void Requisicao_nao_expoe_membros_controlados_pelo_dominio()
    {
        var nomes = ObterPropriedadesPublicas(typeof(CriarPedidoRequisicao));

        CollectionAssert.AreEqual(new[] { "Cliente", "Itens" }, nomes);
    }
    [TestMethod]
    public void Resposta_expoe_somente_membros_autorizados()
    {
        var nomes = ObterPropriedadesPublicas(typeof(PedidoResposta));

        CollectionAssert.AreEqual(
            new[] { "Cliente", "Id", "Itens", "Situacao", "ValorTotal" },
            nomes);

        Assert.IsFalse(nomes.Contains(nameof(Pedido.CodigoDeRoteamentoInterno)));
    }
    [TestMethod]
    public void Versao_inicial_expoe_membros_internos_com_setters_publicos()
    {
        var tipo = typeof(PedidoInicial);
        var observacao = tipo.GetProperty(nameof(PedidoInicial.ObservacaoInterna));
        var situacao = tipo.GetProperty(nameof(PedidoInicial.Situacao));
        var valorTotal = tipo.GetProperty(nameof(PedidoInicial.ValorTotal));

        Assert.IsNotNull(observacao?.SetMethod);
        Assert.IsNotNull(situacao?.SetMethod);
        Assert.IsNotNull(valorTotal?.SetMethod);
        Assert.IsTrue(observacao.SetMethod.IsPublic);
        Assert.IsTrue(situacao.SetMethod.IsPublic);
        Assert.IsTrue(valorTotal.SetMethod.IsPublic);
    }
    [TestMethod]
    public void Fronteira_inicial_altera_e_devolve_a_mesma_entidade_recebida()
    {
        var idGerado = Guid.Parse("218a323a‑3aa3‑4a09‑b2c0‑79a32ed294d8");
        var pedidoRecebido = new PedidoInicial
        {
            Cliente = "Cliente A",
            Itens =
            [
                new ItemPedidoInicial
                {
                    Produto = "Teclado",
                    Quantidade = 2,
                    PrecoUnitario = 150m
                }
            ]
        };
        var fronteira = new PedidoEndpointsInicial();
        var devolvido = fronteira.Criar(pedidoRecebido, idGerado);

        Assert.AreSame(pedidoRecebido, devolvido);
        Assert.AreEqual(idGerado, pedidoRecebido.Id);
        Assert.AreEqual(300m, pedidoRecebido.ValorTotal);
        Assert.AreEqual("Aberto", pedidoRecebido.Situacao);
        Assert.AreEqual("Aguardando conferência operacional.", pedidoRecebido.ObservacaoInterna);
    }
    private static PedidoResposta CriarResposta()
    {
        return MapeadorDePedido.ParaResposta(CriarPedidoDoDominio());
    }
    private static Pedido CriarPedidoDoDominio()
    {
        var pedido = new Pedido(
            Guid.Parse("43da9b57‑986b‑4e50‑a0f7‑f7682a0a920d"), "Cliente A");

        pedido.AdicionarItem(new ItemPedido("Teclado", 2, 150m));

        return pedido;
    }
    private static string[] ObterPropriedadesPublicas(Type tipo) =>
        tipo.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(propriedade => propriedade.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();

    // O tipo auxiliar isola uma referência mutável para tornar a cópia superficial observável.
    private sealed record ContratoComListaMutavel(string Nome, List<string> Marcadores);
}
