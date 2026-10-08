namespace Conde.Cliques;

/// <summary>
/// O JUIZ, e ele e a definicao escrita do jeito mais burro possivel.
///
/// Uma clique e um grupo de vertices todos vizinhos entre si. A maior e a maior, e
/// saber que ela e a maior exige olhar todos os subconjuntos. Sao dois elevado ao
/// numero de vertices, e e por isso que ele so existe ate umas duas dezenas.
///
/// Custa isso e precisa custar. Clique maxima e um dos vinte e um problemas da
/// lista de Karp, e e tambem o problema para o qual se conhece o pior resultado de
/// aproximacao que existe: nao da para garantir nem perto da resposta sem
/// resolver tudo. Um juiz rapido para ele seria uma noticia bem maior que este
/// repositorio.
///
/// E ha um SEGUNDO juiz aqui, que nao fala de clique nenhuma: a maior clique de um
/// grafo e o maior conjunto SEM ARESTA do complemento dele. Os dois percorrem os
/// mesmos subconjuntos e perguntam coisas opostas sobre eles.
/// </summary>
public static class ForcaBruta
{
    /// <param name="Tamanho">o tamanho da maior clique</param>
    /// <param name="Clique">uma clique desse tamanho</param>
    /// <param name="Olhadas">quantos subconjuntos foram considerados</param>
    public record Resultado(int Tamanho, int[] Clique, long Olhadas);

    /// <summary>A MAIOR CLIQUE, por forca bruta sobre todos os subconjuntos.</summary>
    public static Resultado Maior(Grafo grafo)
    {
        if (grafo.Vertices > 24) throw new ArgumentException("forca bruta so ate 24 vertices", nameof(grafo));

        var melhor = 0;
        var melhorMascara = 0;
        var olhadas = 0L;

        for (var mascara = 0; mascara < 1 << grafo.Vertices; mascara++)
        {
            var tamanho = System.Numerics.BitOperations.PopCount((uint)mascara);

            if (tamanho <= melhor) continue;

            olhadas++;

            if (!TodosVizinhos(grafo, mascara)) continue;

            melhor = tamanho;
            melhorMascara = mascara;
        }

        var clique = new List<int>();

        for (var v = 0; v < grafo.Vertices; v++)
            if ((melhorMascara & (1 << v)) != 0) clique.Add(v);

        return new Resultado(melhor, [.. clique], olhadas);
    }

    private static bool TodosVizinhos(Grafo grafo, int mascara)
    {
        for (var a = 0; a < grafo.Vertices; a++)
        {
            if ((mascara & (1 << a)) == 0) continue;

            for (var b = a + 1; b < grafo.Vertices; b++)
            {
                if ((mascara & (1 << b)) == 0) continue;
                if (!grafo.Vizinhos(a, b)) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// O MAIOR CONJUNTO SEM ARESTA NENHUMA, por forca bruta.
    ///
    /// Ele e o juiz do outro lado: a maior clique de um grafo e o maior conjunto
    /// sem aresta do COMPLEMENTO dele, e esse fato nao e uma reformulacao do mesmo
    /// calculo, e um fato sobre a operacao de complementar.
    /// </summary>
    public static int MaiorSemAresta(Grafo grafo)
    {
        if (grafo.Vertices > 24) throw new ArgumentException("forca bruta so ate 24 vertices", nameof(grafo));

        var melhor = 0;

        for (var mascara = 0; mascara < 1 << grafo.Vertices; mascara++)
        {
            var tamanho = System.Numerics.BitOperations.PopCount((uint)mascara);

            if (tamanho <= melhor) continue;

            var livre = true;

            for (var a = 0; a < grafo.Vertices && livre; a++)
            {
                if ((mascara & (1 << a)) == 0) continue;

                for (var b = a + 1; b < grafo.Vertices; b++)
                {
                    if ((mascara & (1 << b)) == 0) continue;
                    if (!grafo.Vizinhos(a, b)) continue;

                    livre = false;

                    break;
                }
            }

            if (livre) melhor = tamanho;
        }

        return melhor;
    }

    /// <summary>
    /// TODAS as cliques maximais, por forca bruta sobre os subconjuntos.
    ///
    /// E o juiz do algoritmo de Bron e Kerbosch inteiro, e nao so do tamanho da
    /// maior: ele precisa achar o mesmo CONJUNTO de cliques, e nao so a mesma
    /// quantidade. Duas listas com a mesma contagem e conteudos diferentes seriam o
    /// erro mais facil de nao notar.
    /// </summary>
    public static List<int[]> TodasAsMaximais(Grafo grafo)
    {
        if (grafo.Vertices > 20) throw new ArgumentException("forca bruta so ate 20 vertices", nameof(grafo));

        var todas = new List<int[]>();

        for (var mascara = 0; mascara < 1 << grafo.Vertices; mascara++)
        {
            var vertices = new List<int>();

            for (var v = 0; v < grafo.Vertices; v++)
                if ((mascara & (1 << v)) != 0) vertices.Add(v);

            if (BronKerbosch.EhMaximal(grafo, vertices)) todas.Add([.. vertices]);
        }

        return todas;
    }

    /// <summary>Duas listas de cliques descrevem o mesmo conjunto de cliques?</summary>
    public static bool MesmasCliques(List<int[]> um, List<int[]> outro)
    {
        if (um.Count != outro.Count) return false;

        var deUm = um.Select(c => string.Join(",", c.Order())).OrderBy(s => s, StringComparer.Ordinal);
        var deOutro = outro.Select(c => string.Join(",", c.Order())).OrderBy(s => s, StringComparer.Ordinal);

        return deUm.SequenceEqual(deOutro);
    }
}
