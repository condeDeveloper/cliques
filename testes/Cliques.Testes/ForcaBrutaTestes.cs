using Conde.Cliques;
using Xunit;

namespace Conde.Cliques.Testes;

/// <summary>
/// O juiz tambem precisa de juiz.
///
/// Todo veredicto deste repositorio e "a maior clique tem k". Se a forca bruta
/// tivesse um erro, o algoritmo seria julgado contra uma referencia errada, e o
/// erro mais provavel dela e devolver um numero GRANDE demais, aceitando como
/// clique um conjunto que nao e.
///
/// Por isso os testes daqui conferem tres coisas separadas: que o conjunto
/// devolvido E uma clique; que nenhum maior existe; e que um SEGUNDO juiz, que
/// nao fala de clique nenhuma, da o mesmo numero.
/// </summary>
public class ForcaBrutaTestes
{
    [Fact]
    public void OConjuntoDevolvidoEhUmaCliqueDoTamanhoAnunciado()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos(15))
        {
            var r = ForcaBruta.Maior(grafo);

            Assert.Equal(r.Tamanho, r.Clique.Length);
            Assert.True(BronKerbosch.EhClique(grafo, r.Clique), nome);
        }
    }

    /// <summary>
    /// E nenhuma clique MAIOR existe: o teste tenta TODOS os subconjuntos de tamanho
    /// um a mais, sem a poda que a busca usa.
    ///
    /// E a outra metade de "a maior tem k", e deixar so a primeira e o jeito
    /// classico de publicar um limite com cara de resposta.
    /// </summary>
    [Fact]
    public void NenhumaCliqueMaiorExiste()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos(4))
        {
            if (grafo.Vertices > 16) continue;

            var maior = ForcaBruta.Maior(grafo).Tamanho;

            for (var mascara = 0; mascara < 1 << grafo.Vertices; mascara++)
            {
                if (System.Numerics.BitOperations.PopCount((uint)mascara) != maior + 1) continue;

                var vertices = new List<int>();

                for (var v = 0; v < grafo.Vertices; v++)
                    if ((mascara & (1 << v)) != 0) vertices.Add(v);

                Assert.False(BronKerbosch.EhClique(grafo, vertices), $"{nome}: achei uma de {maior + 1}");
            }
        }
    }

    /// <summary>
    /// O SEGUNDO JUIZ: a maior clique de um grafo e o maior conjunto SEM ARESTA do
    /// complemento dele.
    ///
    /// Ele nao fala de clique em lugar nenhum, e o caminho que ele percorre e o
    /// mesmo espaco de subconjuntos com a pergunta INVERTIDA. Quando duas contagens
    /// independentes batem em todo grafo, o que esta certo nao e a implementacao: e
    /// a equivalencia entre os dois problemas.
    /// </summary>
    [Fact]
    public void AMaiorCliqueEhOMaiorConjuntoSemArestaDoComplemento()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos(15))
            Assert.Equal(ForcaBruta.Maior(grafo).Tamanho, ForcaBruta.MaiorSemAresta(grafo.Complemento()));

        for (ulong semente = 1; semente <= 50; semente++)
        {
            var grafo = Grafos.Sorteado(14, 0.5, semente);

            Assert.Equal(ForcaBruta.Maior(grafo).Tamanho, ForcaBruta.MaiorSemAresta(grafo.Complemento()));
        }
    }

    /// <summary>
    /// E os dois NAO sao a mesma coisa no mesmo grafo: a maior clique de G e o maior
    /// conjunto sem aresta de G sao numeros diferentes.
    ///
    /// Esse teste existe para o anterior nao passar por acidente. Se as duas contas
    /// dessem o mesmo numero sempre, sem complementar, a equivalencia que o outro
    /// teste confere seria vazia.
    /// </summary>
    [Fact]
    public void NoMesmoGrafoOsDoisNumerosSaoDiferentes()
    {
        var diferentes = 0;

        for (ulong semente = 1; semente <= 30; semente++)
        {
            var grafo = Grafos.Sorteado(14, 0.5, semente);

            if (ForcaBruta.Maior(grafo).Tamanho != ForcaBruta.MaiorSemAresta(grafo)) diferentes++;
        }

        Assert.True(diferentes > 10, $"so {diferentes} de 30 deram numeros diferentes");
    }

    [Fact]
    public void AForcaBrutaDasMaximaisAchaAsMesmasQueOAlgoritmo()
    {
        for (ulong semente = 1; semente <= 30; semente++)
        {
            var grafo = Grafos.Sorteado(14, 0.4, semente);

            Assert.True(ForcaBruta.MesmasCliques(ForcaBruta.TodasAsMaximais(grafo), BronKerbosch.Todas(grafo)));
        }
    }

    [Fact]
    public void AComparacaoDeListasDeCliquesIgnoraAOrdem()
    {
        Assert.True(ForcaBruta.MesmasCliques([[1, 2], [3]], [[3], [2, 1]]));
        Assert.False(ForcaBruta.MesmasCliques([[1, 2]], [[1, 2], [3]]));
        Assert.False(ForcaBruta.MesmasCliques([[1, 2]], [[1, 3]]));
        Assert.True(ForcaBruta.MesmasCliques([], []));
    }

    [Fact]
    public void NoCompletoAMaiorCliqueEhOGrafoInteiro()
    {
        for (var n = 1; n <= 12; n++)
            Assert.Equal(n, ForcaBruta.Maior(Grafos.Completo(n)).Tamanho);
    }

    [Fact]
    public void NoBipartidoAMaiorCliqueEhUmaArestaSo()
    {
        for (ulong semente = 1; semente <= 20; semente++)
        {
            var grafo = Grafos.Bipartido(6, 6, 0.5, semente);

            Assert.True(ForcaBruta.Maior(grafo).Tamanho <= 2);
        }
    }

    [Fact]
    public void AForcaBrutaRecusaGrafoGrandeDemaisEmVezDeDemorarPraSempre()
    {
        Assert.Throws<ArgumentException>(() => ForcaBruta.Maior(new Grafo(30)));
        Assert.Throws<ArgumentException>(() => ForcaBruta.MaiorSemAresta(new Grafo(30)));
        Assert.Throws<ArgumentException>(() => ForcaBruta.TodasAsMaximais(new Grafo(25)));
    }

    [Fact]
    public void OGrafoNaoAceitaMaisQueOLimiteDeVertices() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new Grafo(400));
}
