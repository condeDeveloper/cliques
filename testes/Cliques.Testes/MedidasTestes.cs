using Conde.Cliques;
using Xunit;

namespace Conde.Cliques.Testes;

/// <summary>
/// Os testes das TABELAS que o medidor imprime.
///
/// Eles existem porque numero em README que ninguem confere envelhece errado em
/// silencio: alguem troca uma constante, a tabela muda e o texto continua dizendo
/// a frase antiga. Aqui o teste afirma sobre o mesmo objeto que vira linha
/// impressa.
/// </summary>
public class MedidasTestes
{
    /// <summary>
    /// A linha principal: as tres respostas, de tres lugares diferentes, batem em
    /// toda linha.
    /// </summary>
    [Fact]
    public void AsTresRespostasBatemEmTodaLinha()
    {
        var linhas = Medidas.ContraADefinicao();

        Assert.Equal(26, linhas.Count);
        Assert.All(linhas, l => Assert.Equal(l.MaiorPelaForcaBruta, l.MaiorPeloAlgoritmo));
        Assert.All(linhas, l => Assert.Equal(l.MaiorPelaForcaBruta, l.PeloComplemento));
        Assert.All(linhas, l => Assert.True(l.AsListasBatem, l.Grafo));
    }

    /// <summary>
    /// A frase do texto: no bipartido a maior clique tem DOIS, e no de Moon e Moser
    /// ela tem o numero de grupos.
    /// </summary>
    [Fact]
    public void OsCasosConhecidosDaoOQuePrometem()
    {
        var linhas = Medidas.ContraADefinicao();

        Assert.Equal(10, linhas.Single(l => l.Grafo == "completo de 10").MaiorPelaForcaBruta);
        Assert.Equal(2, linhas.Single(l => l.Grafo == "ciclo de 12").MaiorPelaForcaBruta);
        Assert.Equal(2, linhas.Single(l => l.Grafo == "bipartido 6x6").MaiorPelaForcaBruta);
        Assert.Equal(1, linhas.Single(l => l.Grafo == "sem aresta").MaiorPelaForcaBruta);

        var moonMoser = linhas.Single(l => l.Grafo == "Moon e Moser (4 grupos)");

        Assert.Equal(4, moonMoser.MaiorPelaForcaBruta);
        Assert.Equal(81, moonMoser.Maximais);
    }

    /// <summary>
    /// A frase do texto: o grafo de Moon e Moser ATINGE o limite de tres elevado a n
    /// sobre tres, em toda linha.
    /// </summary>
    [Fact]
    public void OMoonMoserAtingeOLimiteEmTodaLinha()
    {
        var linhas = Medidas.MoonMoser();

        Assert.Equal(10, linhas.Count);
        Assert.All(linhas, l => Assert.Equal(l.OLimite, l.Medidas));
        Assert.All(linhas, l => Assert.Equal(l.Grupos, l.MaiorClique));
        Assert.All(linhas, l => Assert.Equal(3 * l.Grupos, l.Vertices));

        var dez = linhas.Single(l => l.Grupos == 10);

        Assert.Equal(30, dez.Vertices);
        Assert.Equal(59_049, dez.Medidas);
    }

    /// <summary>
    /// A frase do texto: no grafo completo o pivo corta de um milhao de nos para
    /// vinte e um, e no ciclo ele nao corta nada.
    /// </summary>
    [Fact]
    public void OPivoCortaMuitoNumLugarENadaEmOutro()
    {
        var linhas = Medidas.OPivo();

        Assert.Equal(8, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.ComPivo <= l.SemPivo, l.Grafo));

        var completo = linhas.Single(l => l.Grafo == "completo de 20");

        Assert.Equal(1, completo.Maximais);
        Assert.Equal(1_048_576, completo.SemPivo);
        Assert.Equal(21, completo.ComPivo);
        Assert.True(completo.QuantasVezesMenos > 10_000);

        var ciclo = linhas.Single(l => l.Grafo == "ciclo de 40");

        Assert.InRange(ciclo.QuantasVezesMenos, 0.9, 1.2);

        var denso = linhas.Single(l => l.Grafo == "sorteado 60 (80%)");

        Assert.True(denso.QuantasVezesMenos > 100, $"cortou {denso.QuantasVezesMenos:F1} vezes");
    }

    /// <summary>
    /// A frase do texto: o numero de cliques maximais sobe ate quase a densidade
    /// maxima e DESABA no grafo completo.
    /// </summary>
    [Fact]
    public void AQuantidadeDeCliquesSobeEDesaba()
    {
        var linhas = Medidas.Densidade();

        Assert.Equal(9, linhas.Count);

        var pico = linhas.MaxBy(l => l.Maximais)!;

        Assert.Equal(0.9, pico.Chance);
        Assert.True(pico.Maximais > 500_000, $"o pico foi {pico.Maximais}");

        // e no completo sobra UMA clique maximal, que e o grafo inteiro
        var completo = linhas.Single(l => l.Chance == 1.0);

        Assert.Equal(1, completo.Maximais);
        Assert.Equal(60, completo.MaiorClique);

        // a curva sobe ate o pico e cai depois dele
        var antes = linhas.TakeWhile(l => l.Chance < 0.9).ToList();

        for (var i = 1; i < antes.Count; i++)
            Assert.True(antes[i].Maximais > antes[i - 1].Maximais);

        var depois = linhas.SkipWhile(l => l.Chance <= 0.9).ToList();

        for (var i = 1; i < depois.Count; i++)
            Assert.True(depois[i].Maximais < depois[i - 1].Maximais);
    }

    /// <summary>
    /// E a MAIOR clique so cresce com a densidade, enquanto a QUANTIDADE delas sobe
    /// e desce: sao duas coisas diferentes que o mesmo algoritmo entrega junto.
    /// </summary>
    [Fact]
    public void AMaiorCliqueSoCresceComADensidade()
    {
        var linhas = Medidas.Densidade();

        for (var i = 1; i < linhas.Count; i++)
            Assert.True(linhas[i].MaiorClique >= linhas[i - 1].MaiorClique,
                $"{linhas[i].Chance:F2}: {linhas[i].MaiorClique} contra {linhas[i - 1].MaiorClique}");
    }

    /// <summary>
    /// A frase do texto: nos grafos grandes o numero de nos acompanha o numero de
    /// cliques, e nao o tamanho do grafo.
    /// </summary>
    [Fact]
    public void NosGrafosGrandesOCustoAcompanhaARespostaENaoAEntrada()
    {
        var linhas = Medidas.Grandes();

        Assert.Equal(8, linhas.Count);

        // o completo de 60 tem mais arestas que o ciclo de 300 e custa menos
        var completo = linhas.Single(l => l.Grafo == "completo de 60");
        var ciclo = linhas.Single(l => l.Grafo == "ciclo de 300");

        Assert.Equal(1, completo.Maximais);
        Assert.Equal(60, completo.MaiorClique);
        Assert.Equal(61, completo.Nos);
        Assert.True(completo.Arestas > ciclo.Arestas);
        Assert.True(completo.Nos < ciclo.Nos);

        // e o numero de nos fica sempre na casa do numero de cliques
        Assert.All(linhas, l => Assert.True(l.Nos >= l.Maximais, l.Grafo));
    }
}
