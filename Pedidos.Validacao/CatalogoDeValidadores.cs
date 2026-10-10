using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Pedidos.Validacao;
namespace Pedidos.Validacao;

public interface IValidadorRegistrado
{
    ResultadoValidacao Validar(object? comando);
}
public sealed class DiagnosticoDaDescoberta
{
    internal DiagnosticoDaDescoberta(
            Type[] tiposEncontrados,
            Type[] tiposConcretos,
            Type[] tiposCompativeis,
            Type[] tiposMarcados)
    {
        TiposEncontrados = tiposEncontrados;
        TiposConcretos = tiposConcretos;
        TiposCompativeis = tiposCompativeis;
        TiposMarcados = tiposMarcados;
    }
    public IReadOnlyList<Type> TiposEncontrados { get; }
    public IReadOnlyList<Type> TiposConcretos { get; }
    public IReadOnlyList<Type> TiposCompativeis { get; }
    public IReadOnlyList<Type> TiposMarcados { get; }
}

// O catálogo recebe um único assembly autorizado e guarda as instâncias descobertas.
public sealed class CatalogoDeValidadores
{
    private readonly IReadOnlyDictionary<string, IValidadorRegistrado> _validadores;
    public CatalogoDeValidadores(Assembly assemblyAutorizado)
    {
        ArgumentNullException.ThrowIfNull(assemblyAutorizado);
        var tiposEncontrados = ObterTipos(assemblyAutorizado)
            .OrderBy(tipo => tipo.FullName, StringComparer.Ordinal)
            .ToArray();

        // Cada etapa fica registrada para mostrar exatamente o limite da  descoberta.
        var tiposConcretos = tiposEncontrados
            .Where(tipo => tipo is { IsClass: true, IsAbstract: false })
            .ToArray();

        var contrato = typeof(IValidadorRegistrado);

        var tiposCompativeis = tiposConcretos
            .Where(contrato.IsAssignableFrom)
            .ToArray();

        var tiposMarcados = tiposCompativeis
            .Where(tipo => tipo.GetCustomAttribute<ValidadorRegistradoAttribute>(inherit:false) is not null)
            .ToArray();

        Diagnostico = new DiagnosticoDaDescoberta(
            tiposEncontrados,
            tiposConcretos,
            tiposCompativeis,
            tiposMarcados);

        _validadores = CriarValidadores(tiposMarcados);
    }
    public DiagnosticoDaDescoberta Diagnostico { get; }
    public IValidadorRegistrado Obter(string chave)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chave);
        if (_validadores.TryGetValue(chave, out var validador))
        {
            // A instância vem do dicionário; não há nova varredura reflexiva.
            return validador;
        }
        throw new KeyNotFoundException(
            $"Não existe validador registrado para a chave '{chave}'.");
    }
    private static Type[] ObterTipos(Assembly assemblyAutorizado)
    {
        try
        {
            // Somente o assembly recebido é examinado.
            return assemblyAutorizado.GetTypes();
        }
        catch (ReflectionTypeLoadException excecao)
        {
            throw new InvalidOperationException(
                $"Não foi possível examinar todos os tipos do assembly " +
                $"'{assemblyAutorizado.GetName().Name}'.",
                excecao);
        }
    }
    private static IReadOnlyDictionary<string, IValidadorRegistrado>CriarValidadores(IEnumerable<Type> tiposMarcados)
    {
        var validadores =
            new Dictionary<string, IValidadorRegistrado>(StringComparer.Ordinal);
        var tiposPorChave =
            new Dictionary<string, Type>(StringComparer.Ordinal);
        foreach (var tipo in tiposMarcados)
        {
            var atributo = tipo.GetCustomAttribute<ValidadorRegistradoAttribute>(inherit:false)!;
            if (tiposPorChave.TryGetValue(atributo.Chave, out var
            tipoExistente))
            {
                throw new InvalidOperationException(
                    $"A chave '{atributo.Chave}' foi registrada pelos tipos " +
                    $"'{tipoExistente.FullName}' e '{tipo.FullName}'.");
            }
            if (tipo.GetConstructor(Type.EmptyTypes) is null)
            {
                throw new InvalidOperationException(
                    $"O tipo '{tipo.FullName}' precisa declarar um construtor " +
                    $"público sem parâmetros.");
            }
            // A reflexão cria a instância; o uso posterior ocorre pela interface.
            var instancia = (IValidadorRegistrado)Activator.CreateInstance(tipo)!;

            tiposPorChave.Add(atributo.Chave, tipo);
            validadores.Add(atributo.Chave, instancia);
        }
        return validadores;
    }
}

[ValidadorRegistrado("criar-pedido")]
public sealed class ValidadorRegistradoDeCriacao : IValidadorRegistrado
{
    private readonly ValidadorDeComandos _validador = new();
    public ResultadoValidacao Validar(object? comando) =>
        _validador.Validar(comando);
}

// Os três tipos seguintes tornam observável a ação de cada filtro.
public sealed class ValidadorCompativelNaoMarcado : IValidadorRegistrado
{
    public ResultadoValidacao Validar(object? comando) =>
        ResultadoValidacao.Rejeitar("Validador não registrado.");
}

[ValidadorRegistrado("incompativel")]
public sealed class TipoMarcadoIncompativel
{
}

[ValidadorRegistrado("abstrato")]
public abstract class ValidadorAbstratoMarcado : IValidadorRegistrado
{
    public abstract ResultadoValidacao Validar(object? comando);
}
