using Conde.Cliques;
using Xunit;

namespace Conde.Cliques.Testes;

public class GrafoTestes
{
    [Fact]
    public void OSorteioEhDeterminista()
    {
        var a = new Sorteio(7);
        var b = new Sorteio(7);

        for (var i = 0; i < 1000; i++) Assert.Equal(a.Proximo(), b.Proximo());
    }

    /// <summary>
    /// A fracao precisa cobrir os DOIS LADOS do meio: um deslocamento de 33 em vez
    /// de 32 deixaria o gerador com 31 bits, e "sortear uma aresta com 80% de
    /// chance" viraria "sortear sempre", com os numeros continuando plausiveis.
    /// </summary>
    [Fact]
    public void AFracaoCobreOsDoisLadosDoMeio()
    {
        var sorteio = new Sorteio(7);
        var acima = 0;
        var maior = 0.0;

        for (var i = 0; i < 10_000; i++)
        {
            var f = sorteio.Fracao();

            Assert.InRange(f, 0.0, 1.0);
            if (f > 0.5) acima++;
            if (f > maior) maior = f;
        }

        Assert.InRange(acima, 4_700, 5_300);
        Assert.True(maior > 0.999, $"o maior sorteio foi {maior}");
    }

    [Fact]
    public void LigarDuasVezesNaoCriaDuasArestas()
    {
        var grafo = new Grafo(3);

        grafo.Ligar(0, 1);
        grafo.Ligar(1, 0);

        Assert.Equal(1, grafo.Arestas);
        Assert.True(grafo.Vizinhos(1, 0));
        Assert.False(grafo.Vizinhos(0, 2));
    }

    [Fact]
    public void LacoNaoEhGrafoSimples() =>
        Assert.Throws<ArgumentException>(() => new Grafo(3).Ligar(1, 1));

    /// <summary>
    /// A vizinhanca guardada em BITS precisa funcionar ACIMA de 64 vertices, que e
    /// onde ela passa a ocupar mais de uma palavra.
    ///
    /// E o unico lugar onde a estrutura de dados pode errar em silencio: com
    /// sessenta e quatro vertices tudo cabe numa palavra e nenhum teste pequeno
    /// toca na segunda.
    /// </summary>
    [Fact]
    public void OsBitsFuncionamAcimaDeSessentaEQuatroVertices()
    {
        var grafo = new Grafo(200);

        Assert.Equal(4, grafo.Palavras);

        grafo.Ligar(3, 190);
        grafo.Ligar(65, 130);

        Assert.True(grafo.Vizinhos(3, 190));
        Assert.True(grafo.Vizinhos(190, 3));
        Assert.True(grafo.Vizinhos(65, 130));
        Assert.False(grafo.Vizinhos(3, 65));
        Assert.Equal(1, grafo.Grau(190));
        Assert.Equal(1, grafo.Grau(3));
    }

    [Fact]
    public void OsConjuntosDeBitsFazemOQueDizem()
    {
        var conjunto = new ulong[3];

        Assert.True(Bits.Vazio(conjunto));
        Assert.Equal(0, Bits.Quantos(conjunto));

        Bits.Acender(conjunto, 5);
        Bits.Acender(conjunto, 130);

        Assert.False(Bits.Vazio(conjunto));
        Assert.Equal(2, Bits.Quantos(conjunto));
        Assert.True(Bits.Tem(conjunto, 130));
        Assert.Equal([5, 130], Bits.Membros(conjunto));

        Bits.Apagar(conjunto, 5);

        Assert.Equal([130], Bits.Membros(conjunto));
    }

    [Fact]
    public void AIntersecaoDeBitsEhAIntersecao()
    {
        var um = new ulong[2];
        var outro = new ulong[2];

        foreach (var v in new[] { 1, 5, 70 }) Bits.Acender(um, v);
        foreach (var v in new[] { 5, 70, 99 }) Bits.Acender(outro, v);

        Assert.Equal([5, 70], Bits.Membros(Bits.E(um, outro)));
    }

    /// <summary>
    /// O COMPLEMENTO troca as arestas pelas que faltavam, e o complemento do
    /// complemento e o grafo de volta.
    /// </summary>
    [Fact]
    public void OComplementoDoComplementoEhOOriginal()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos(5))
        {
            var voltou = grafo.Complemento().Complemento();

            Assert.Equal(grafo.Arestas, voltou.Arestas);

            for (var a = 0; a < grafo.Vertices; a++)
                for (var b = 0; b < grafo.Vertices; b++)
                    if (a != b) Assert.Equal(grafo.Vizinhos(a, b), voltou.Vizinhos(a, b));
        }
    }

    [Fact]
    public void OComplementoTemAsArestasQueFaltavam()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos(5))
        {
            var possiveis = grafo.Vertices * (grafo.Vertices - 1) / 2;

            Assert.Equal(possiveis - grafo.Arestas, grafo.Complemento().Arestas);
        }
    }

    /// <summary>
    /// O GRAFO DE MOON e MOSER tem a maior clique do tamanho do numero de grupos, e
    /// tres elevado a esse numero cliques maximais.
    ///
    /// Os dois numeros sao previstos pela construcao e conferidos pelo algoritmo, e
    /// sao eles que fazem o grafo valer a pena.
    /// </summary>
    [Fact]
    public void OMoonMoserTemOQuePrometeTer()
    {
        for (var grupos = 1; grupos <= 8; grupos++)
        {
            var (grafo, maior, quantas) = Grafos.MoonMoser(grupos);
            var r = BronKerbosch.Listar(grafo);

            Assert.Equal(3 * grupos, grafo.Vertices);
            Assert.Equal(maior, r.MaiorClique);
            Assert.Equal(quantas, r.Quantas);
        }
    }

    [Fact]
    public void OsGeradoresDaoOQueOsNomesDizem()
    {
        Assert.Equal(45, Grafos.Completo(10).Arestas);
        Assert.Equal(12, Grafos.Ciclo(12).Arestas);
        Assert.Equal(54, Grafos.MoonMoser(4).Grafo.Arestas);
        Assert.Equal(0, new Grafo(10).Arestas);

        var (turan, maior) = Grafos.Turan(12, 4);

        Assert.Equal(4, maior);
        Assert.Equal(4, ForcaBruta.Maior(turan).Tamanho);
    }

    /// <summary>
    /// A CLIQUE PLANTADA esta la mesmo, e a forca bruta nao acha nada menor que
    /// ela.
    /// </summary>
    [Fact]
    public void ACliquePlantadaEstaLa()
    {
        for (ulong semente = 1; semente <= 10; semente++)
        {
            var (grafo, plantada) = Grafos.ComCliquePlantada(18, 7, 0.2, semente);

            Assert.True(ForcaBruta.Maior(grafo).Tamanho >= plantada);
            Assert.True(BronKerbosch.MaiorClique(grafo) >= plantada);
        }
    }
}
