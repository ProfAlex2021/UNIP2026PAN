using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Mono.Cecil.Cil;

[assembly: DoNotParallelize]
namespace Pedidos.Validacao.Testes;

[TestClass]
public sealed class ValidadorDeComandosTestes
{
    [TestMethod]
    public void Versoes_produzem_o_mesmo_resultado_para_toda_a_matriz()
    {

        object?[] comandos =
        [
            null,
            new CriarPedido(clienteAtivo: true, valorTotal: 1m, quantidadeItens: 1),
            new CriarPedido(
                clienteAtivo: true,
                valorTotal: 10_000m,
                quantidadeItens: 100),
            new CriarPedido(
                clienteAtivo: false,
                valorTotal: 100m,
                quantidadeItens: 2),
            new CriarPedido(
                clienteAtivo: true,
                valorTotal: 0m,
                quantidadeItens: 2),
            new CriarPedido(
                clienteAtivo: true,
                valorTotal: 10_000.01m,
                quantidadeItens: 2),
            new CriarPedido(
                clienteAtivo: true,
                valorTotal: 100m,
                quantidadeItens: 0),
            new CriarPedido(
                clienteAtivo: true,
                valorTotal: 100m,
                quantidadeItens: 101),
            new CancelarPedido(pedidoEncontrado: false, jaEnviado: false),
            new CancelarPedido(pedidoEncontrado: true, jaEnviado: true),
            new CancelarPedido(pedidoEncontrado: true, jaEnviado: false),
            new ComandoDesconhecido()
        ];

        var inicial = new ValidadorDeComandosInicial();
        var revisado = new ValidadorDeComandos();

        foreach (var comando in comandos)
        {
            var resultadoInicial = inicial.Validar(comando);
            var resultadoRevisado = revisado.Validar(comando);
            Assert.AreEqual(resultadoInicial.Aceito, resultadoRevisado.Aceito);
            Assert.AreEqual(resultadoInicial.Mensagem, resultadoRevisado.Mensagem);
        }
    }

    [TestMethod]
    public void Criacao_de_pedido_aceita_os_limites_inclusivos()
    {
        var validador = new ValidadorDeComandos();

        var limiteInferior = validador.Validar(
            new CriarPedido(
                clienteAtivo: true,
                valorTotal: 0.01m,
                quantidadeItens: 1));

        var limiteSuperior = validador.Validar(
            new CriarPedido(
                clienteAtivo: true,
                valorTotal: 10_000m,
                quantidadeItens: 100));

        Assert.IsTrue(limiteInferior.Aceito);
        Assert.IsTrue(limiteSuperior.Aceito);
    }

    [TestMethod]
    public void Criacao_de_pedido_rejeita_valores_fora_dos_limites()
    {
        var validador = new ValidadorDeComandos();
        CriarPedido[] comandosInvalidos =
        [
            new(clienteAtivo: true, valorTotal: 0m, quantidadeItens: 1),
            new(clienteAtivo: true, valorTotal: 10_000.01m, quantidadeItens: 1),
            new(clienteAtivo: true, valorTotal: 1m, quantidadeItens: 0),
            new(clienteAtivo: true, valorTotal: 1m, quantidadeItens: 101)
        ];
        foreach (var comando in comandosInvalidos)
        {
            Assert.IsFalse(validador.Validar(comando).Aceito);
        }
    }
    [TestMethod]
    public void Entrada_nula_e_tipo_desconhecido_sao_tratados_explicitamente()
    {
        var validador = new ValidadorDeComandos();
        var nulo = validador.Validar(null);
        var desconhecido = validador.Validar(new ComandoDesconhecido());
        Assert.IsFalse(nulo.Aceito);
        Assert.AreEqual("O comando não pode ser nulo.", nulo.Mensagem);
        Assert.IsFalse(desconhecido.Aceito);
        Assert.AreEqual(
            "O tipo de comando não é suportado.",
            desconhecido.Mensagem);

    }

    [TestMethod]
    public void Catalogo_mantem_somente_tipos_concretos_compativeis_e_marcados()
    {
        var catalogo =
            new CatalogoDeValidadores(typeof(CatalogoDeValidadores).Assembly);

        var diagnostico = catalogo.Diagnostico;

        Console.WriteLine(
            $"Tipos encontrados: { FormatarTipos(diagnostico.TiposEncontrados)}");

        Console.WriteLine(
            $"Tipos concretos: { FormatarTipos(diagnostico.TiposConcretos)}");

        Console.WriteLine(
            $"Tipos compatíveis: { FormatarTipos(diagnostico.TiposCompativeis)}");

        Console.WriteLine(
            $"Tipos marcados: { FormatarTipos(diagnostico.TiposMarcados)}");

        Assert.IsTrue(
            diagnostico.TiposEncontrados.Contains(
                typeof(ValidadorAbstratoMarcado)));

        Assert.IsFalse(
            diagnostico.TiposConcretos.Contains(
                typeof(ValidadorAbstratoMarcado)));

        Assert.IsTrue(
            diagnostico.TiposConcretos.Contains(
                typeof(TipoMarcadoIncompativel)));

        Assert.IsFalse(
            diagnostico.TiposCompativeis.Contains(
                typeof(TipoMarcadoIncompativel)));

        Assert.IsTrue(
            diagnostico.TiposCompativeis.Contains(
                typeof(ValidadorCompativelNaoMarcado)));

        Assert.IsFalse(
            diagnostico.TiposMarcados.Contains(
                typeof(ValidadorCompativelNaoMarcado)));

        CollectionAssert.AreEqual(
            new[] { typeof(ValidadorRegistradoDeCriacao) },
            diagnostico.TiposMarcados.ToArray());
    }

    [TestMethod]
    public void Catalogo_reutiliza_a_instancia_descoberta()
    {
        var catalogo =
            new CatalogoDeValidadores(typeof(CatalogoDeValidadores).Assembly);

        var primeiraConsulta = catalogo.Obter("criar-pedido");
        var segundaConsulta = catalogo.Obter("criar-pedido");

        Assert.AreSame(primeiraConsulta, segundaConsulta);

        Assert.IsTrue(
            primeiraConsulta.Validar(
                new CriarPedido(
                    clienteAtivo: true,
                    valorTotal: 50m,
                    quantidadeItens: 2)).Aceito);
    }

    [TestMethod]
    public void Catalogo_rejeita_chave_duplicada_com_mensagem_deterministica()
    {
        var nomesOrdenados = new[]
        {
            typeof(PrimeiroValidadorDuplicado).FullName!,
            typeof(SegundoValidadorDuplicado).FullName!
        }
        .Order(StringComparer.Ordinal)
        .ToArray();
        
        var excecao = Assert.ThrowsExactly<InvalidOperationException>(
            () => new CatalogoDeValidadores(Assembly.GetExecutingAssembly()));

        Assert.AreEqual(
            $"A chave 'duplicada' foi registrada pelos tipos " +
            $"'{ nomesOrdenados[0]}' e '{ nomesOrdenados[1]}'.",
            excecao.Message);
    }
    [TestMethod]
    public void Catalogo_informa_chave_inexistente_com_mensagem_deterministica()
    {
        var catalogo =
            new CatalogoDeValidadores(typeof(CatalogoDeValidadores).Assembly);

        var excecao = Assert.ThrowsExactly<KeyNotFoundException>(
            () => catalogo.Obter("nao‑existe"));

        Assert.AreEqual(
            "Não existe validador registrado para a chave 'nao‑existe'.",
            excecao.Message);
    }

    [TestMethod]
    public void Atributo_restringe_destino_multiplicidade_e_heranca()
    {
        var uso = typeof(ValidadorRegistradoAttribute)
                    .GetCustomAttribute<AttributeUsageAttribute>();
        Assert.IsNotNull(uso);
        Assert.AreEqual(AttributeTargets.Class, uso.ValidOn);
        Assert.IsFalse(uso.AllowMultiple);
        Assert.IsFalse(uso.Inherited);
    }

    private sealed class ComandoDesconhecido
    {
    }

    [ValidadorRegistrado("duplicada")]
    public sealed class PrimeiroValidadorDuplicado : IValidadorRegistrado
    {
        public ResultadoValidacao Validar(object? comando) =>
                    ResultadoValidacao.Rejeitar("Não utilizado neste teste.");
    }

    [ValidadorRegistrado("duplicada")]
    public sealed class SegundoValidadorDuplicado : IValidadorRegistrado
    {
        public ResultadoValidacao Validar(object? comando) =>
                    ResultadoValidacao.Rejeitar("Não utilizado neste teste.");
    }
    private static string FormatarTipos(IEnumerable<Type> tipos) =>
        string.Join(", ", tipos.Select(tipo => tipo.Name));
}


