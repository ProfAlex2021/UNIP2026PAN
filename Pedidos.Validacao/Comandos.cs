using System;
using System.Collections.Generic;
using System.Text;

namespace Pedidos.Validacao;

public sealed class CriarPedido
{
    public CriarPedido(bool clienteAtivo, decimal valorTotal, int quantidadeItens)
    {
        ClienteAtivo = clienteAtivo;
        ValorTotal = valorTotal;
        QuantidadeItens = quantidadeItens;
    }
    public bool ClienteAtivo { get; }
    public decimal ValorTotal { get; }
    public int QuantidadeItens { get; }
}
public sealed class CancelarPedido
{
    public CancelarPedido(bool pedidoEncontrado, bool jaEnviado)
    {
        PedidoEncontrado = pedidoEncontrado;
        JaEnviado = jaEnviado;
    }
    public bool PedidoEncontrado { get; }
    public bool JaEnviado { get; }
}
