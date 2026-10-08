namespace Conde.Cliques;

/// <summary>
/// As TABELAS que o medidor imprime e que os testes conferem.
///
/// Elas ficam aqui, e nao dentro do programa do medidor, por um motivo so: numero
/// em README que ninguem confere envelhece errado em silencio. Alguem troca uma
/// constante, a tabela muda, e o texto continua afirmando a frase antiga. Aqui o
/// teste afirma sobre o MESMO objeto que vira linha impressa.
/// </summary>
public static class Medidas
{
    public record LinhaDefinicao(
        string Grafo, int Vertices, int Arestas, int MaiorPelaForcaBruta,
        int MaiorPeloAlgoritmo, int PeloComplemento, long Maximais, bool AsListasBatem);

    /// <summary>
    /// O algoritmo contra a DEFINICAO, nos grafos em que ela cabe.
    ///
    /// Tres respostas de tres lugares: a forca bruta sobre subconjuntos, a busca de
    /// Bron e Kerbosch, e o maior conjunto sem aresta do complemento.
    /// </summary>
    public static List<LinhaDefinicao> ContraADefinicao(int sorteados = 10)
    {
        var linhas = new List<LinhaDefinicao>();

        foreach (var (nome, grafo) in Grafos.Pequenos(sorteados))
        {
            var r = BronKerbosch.Listar(grafo, true, null);
            var doAlgoritmo = BronKerbosch.Todas(grafo);
            var batem = grafo.Vertices <= 20 && ForcaBruta.MesmasCliques(doAlgoritmo, ForcaBruta.TodasAsMaximais(grafo));

            linhas.Add(new LinhaDefinicao(
                nome, grafo.Vertices, grafo.Arestas,
                ForcaBruta.Maior(grafo).Tamanho, r.MaiorClique,
                ForcaBruta.MaiorSemAresta(grafo.Complemento()),
                r.Quantas, batem));
        }

        return linhas;
    }

    public record LinhaPivo(string Grafo, int Vertices, long Maximais, long SemPivo, long ComPivo, double QuantasVezesMenos);

    /// <summary>
    /// O PIVO contra a falta dele, em nos da busca.
    ///
    /// Nenhum segundo em lugar nenhum: segundo depende de maquina, de compilador e
    /// de quem mais esta rodando. No da busca e o mesmo numero nos tres sistemas da
    /// integracao continua.
    /// </summary>
    public static List<LinhaPivo> OPivo()
    {
        var linhas = new List<LinhaPivo>();

        foreach (var (nome, grafo) in new (string, Grafo)[]
        {
            ("completo de 20", Grafos.Completo(20)),
            ("ciclo de 40", Grafos.Ciclo(40)),
            ("bipartido 20x20", Grafos.Bipartido(20, 20, 0.4, 7)),
            ("Moon e Moser (5 grupos)", Grafos.MoonMoser(5).Grafo),
            ("Moon e Moser (8 grupos)", Grafos.MoonMoser(8).Grafo),
            ("sorteado 60 (20%)", Grafos.Sorteado(60, 0.2, 11)),
            ("sorteado 60 (50%)", Grafos.Sorteado(60, 0.5, 13)),
            ("sorteado 60 (80%)", Grafos.Sorteado(60, 0.8, 17)),
        })
        {
            var sem = BronKerbosch.Listar(grafo, false, null);
            var com = BronKerbosch.Listar(grafo, true, null);

            linhas.Add(new LinhaPivo(
                nome, grafo.Vertices, com.Quantas, sem.Nos, com.Nos, (double)sem.Nos / com.Nos));
        }

        return linhas;
    }

    public record LinhaMoonMoser(int Grupos, int Vertices, long OLimite, long Medidas, int MaiorClique, long Nos);

    /// <summary>
    /// O GRAFO DE MOON e MOSER contra o limite deles.
    ///
    /// Moon e Moser provaram em 1965 que nenhum grafo de n vertices tem mais que
    /// tres elevado a n sobre tres cliques maximais. Este grafo ATINGE o limite, e e
    /// essa a graca: ele nao e um caso ruim entre outros, ele e O caso ruim, e
    /// nenhum algoritmo que liste todas as cliques pode escapar dele.
    /// </summary>
    public static List<LinhaMoonMoser> MoonMoser(int ate = 10)
    {
        var linhas = new List<LinhaMoonMoser>();

        for (var grupos = 1; grupos <= ate; grupos++)
        {
            var (grafo, maior, limite) = Grafos.MoonMoser(grupos);
            var r = BronKerbosch.Listar(grafo);

            linhas.Add(new LinhaMoonMoser(grupos, grafo.Vertices, limite, r.Quantas, r.MaiorClique, r.Nos));
        }

        return linhas;
    }

    public record LinhaDensidade(
        int Vertices, double Chance, int Arestas, long Maximais, int MaiorClique, long Nos, double NosPorClique);

    /// <summary>
    /// Quantas cliques maximais um grafo sorteado tem, com a densidade crescendo.
    ///
    /// A curva nao e monotona, e isso e o interessante: com poucas arestas quase
    /// toda clique maximal e uma aresta sozinha, e com muitas o grafo inteiro vira
    /// quase uma clique so. O pico fica no meio.
    /// </summary>
    public static List<LinhaDensidade> Densidade(int vertices = 60)
    {
        var linhas = new List<LinhaDensidade>();

        foreach (var chance in new[] { 0.1, 0.3, 0.5, 0.7, 0.8, 0.9, 0.95, 0.99, 1.0 })
        {
            var grafo = Grafos.Sorteado(vertices, chance, 7);
            var r = BronKerbosch.Listar(grafo);

            linhas.Add(new LinhaDensidade(
                vertices, chance, grafo.Arestas, r.Quantas, r.MaiorClique, r.Nos, (double)r.Nos / r.Quantas));
        }

        return linhas;
    }

    public record LinhaGrande(string Grafo, int Vertices, int Arestas, long Maximais, int MaiorClique, long Nos);

    /// <summary>
    /// Os grafos grandes, onde a forca bruta nao chega.
    ///
    /// Aqui a maior clique vem do proprio algoritmo, e nao de um juiz. E isso esta
    /// dito na tabela de proposito: a coluna se chama "maior clique achada", e nao
    /// "maior clique", porque a segunda seria uma afirmacao que este repositorio nao
    /// tem como sustentar nesses tamanhos.
    ///
    /// O que da para sustentar e que ela E uma clique, e que ela e maximal, e as
    /// duas coisas sao conferidas em teste.
    /// </summary>
    public static List<LinhaGrande> Grandes()
    {
        var linhas = new List<LinhaGrande>();

        foreach (var (nome, grafo) in Grafos.Grandes())
        {
            var r = BronKerbosch.Listar(grafo);

            linhas.Add(new LinhaGrande(nome, grafo.Vertices, grafo.Arestas, r.Quantas, r.MaiorClique, r.Nos));
        }

        return linhas;
    }
}
