namespace Pedidos.Validacao;
// Cada classe pode declarar uma única chave, sem herdá‑la de uma classe‑base.
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ValidadorRegistradoAttribute : Attribute
{
    public ValidadorRegistradoAttribute(string chave)
    {
        if (string.IsNullOrWhiteSpace(chave))
        {
            throw new ArgumentException(
                "A chave do validador deve ser informada.",
                nameof(chave));
        }

        Chave = chave;
    }
    public string Chave { get; }
}
