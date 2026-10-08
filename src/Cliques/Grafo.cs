namespace Conde.Cliques;

/// <summary>
/// O SORTEIO, escrito aqui em vez de vir da biblioteca.
///
/// A documentacao do sorteio de biblioteca nao promete a mesma sequencia entre
/// versoes nem entre sistemas, e a integracao continua roda em tres. "O pivo
/// cortou de 1.864 nos para 212" com grafos diferentes em cada sistema nao e uma
/// medida: sao tres medidas que ninguem pode comparar.
/// </summary>
public sealed class Sorteio
{
    private ulong estado;

    public Sorteio(ulong semente) => estado = semente == 0 ? 1 : semente;

    public uint Proximo()
    {
        estado = estado * 6364136223846793005UL + 1442695040888963407UL;
        return (uint)(estado >> 32);
    }

    public int Ate(int limite) => (int)(Proximo() % (uint)limite);

    public double Fracao() => Proximo() / 4294967296.0;
}

/// <summary>
/// Um GRAFO simples, com a vizinhanca guardada em BITS.
///
/// Uma CLIQUE e um grupo de vertices todos vizinhos entre si. Procurar a maior e
/// um dos vinte e um problemas da lista de Karp de 1972, e e tambem o problema
/// para o qual se sabe o pior resultado de aproximacao que existe: nao da para
/// garantir nem perto da resposta sem resolver tudo.
///
/// Os bits nao sao preciosismo. O algoritmo de Bron e Kerbosch passa a vida
/// INTERSECTANDO conjuntos de vertices, e com bits isso e um "e" por palavra de
/// sessenta e quatro. Com lista de vizinhos seria um laco, e o numero medido
/// deixaria de falar do algoritmo para falar da estrutura de dados.
/// </summary>
public sealed class Grafo
{
    private readonly ulong[][] vizinhos;

    public Grafo(int vertices)
    {
        if (vertices > 320) throw new ArgumentOutOfRangeException(nameof(vertices), "este repositorio vai ate 320 vertices");

        Vertices = vertices;
        Palavras = (vertices + 63) / 64;
        vizinhos = new ulong[vertices][];

        for (var v = 0; v < vertices; v++) vizinhos[v] = new ulong[Palavras];
    }

    public int Vertices { get; }

    /// <summary>Quantas palavras de 64 bits cabem os vertices.</summary>
    public int Palavras { get; }

    public int Arestas { get; private set; }

    public void Ligar(int a, int b)
    {
        if (a == b) throw new ArgumentException("laco nao e grafo simples", nameof(a));
        if (Vizinhos(a, b)) return;

        vizinhos[a][b / 64] |= 1UL << (b % 64);
        vizinhos[b][a / 64] |= 1UL << (a % 64);
        Arestas++;
    }

    public bool Vizinhos(int a, int b) => (vizinhos[a][b / 64] & (1UL << (b % 64))) != 0;

    /// <summary>A vizinhanca de um vertice, como bits.</summary>
    public ulong[] De(int vertice) => vizinhos[vertice];

    public int Grau(int vertice)
    {
        var grau = 0;

        foreach (var palavra in vizinhos[vertice]) grau += System.Numerics.BitOperations.PopCount(palavra);

        return grau;
    }

    public IEnumerable<(int A, int B)> TodasAsArestas()
    {
        for (var a = 0; a < Vertices; a++)
            for (var b = a + 1; b < Vertices; b++)
                if (Vizinhos(a, b)) yield return (a, b);
    }

    /// <summary>
    /// O COMPLEMENTO: as arestas que faltavam viram as que ha, e as que havia
    /// somem.
    ///
    /// Ele e a ponte entre dois problemas que parecem diferentes e sao o mesmo: a
    /// maior clique de um grafo e o maior conjunto SEM aresta nenhuma do
    /// complemento dele. Esse fato e usado aqui como juiz, porque ele produz uma
    /// resposta por um caminho que nao olha para clique nenhuma.
    /// </summary>
    public Grafo Complemento()
    {
        var outro = new Grafo(Vertices);

        for (var a = 0; a < Vertices; a++)
            for (var b = a + 1; b < Vertices; b++)
                if (!Vizinhos(a, b)) outro.Ligar(a, b);

        return outro;
    }

    /// <summary>Um conjunto de vertices com todos os bits acesos.</summary>
    public ulong[] TodosOsVertices()
    {
        var conjunto = new ulong[Palavras];

        for (var v = 0; v < Vertices; v++) conjunto[v / 64] |= 1UL << (v % 64);

        return conjunto;
    }
}

/// <summary>Operacoes sobre conjuntos de vertices guardados em bits.</summary>
public static class Bits
{
    public static int Quantos(ulong[] conjunto)
    {
        var quantos = 0;

        foreach (var palavra in conjunto) quantos += System.Numerics.BitOperations.PopCount(palavra);

        return quantos;
    }

    public static bool Vazio(ulong[] conjunto)
    {
        foreach (var palavra in conjunto)
            if (palavra != 0) return false;

        return true;
    }

    public static bool Tem(ulong[] conjunto, int v) => (conjunto[v / 64] & (1UL << (v % 64))) != 0;

    public static void Acender(ulong[] conjunto, int v) => conjunto[v / 64] |= 1UL << (v % 64);

    public static void Apagar(ulong[] conjunto, int v) => conjunto[v / 64] &= ~(1UL << (v % 64));

    public static ulong[] Copia(ulong[] conjunto) => [.. conjunto];

    /// <summary>A intersecao de dois conjuntos: a operacao que o algoritmo faz o tempo todo.</summary>
    public static ulong[] E(ulong[] um, ulong[] outro)
    {
        var resultado = new ulong[um.Length];

        for (var i = 0; i < um.Length; i++) resultado[i] = um[i] & outro[i];

        return resultado;
    }

    public static IEnumerable<int> Membros(ulong[] conjunto)
    {
        for (var i = 0; i < conjunto.Length; i++)
        {
            var palavra = conjunto[i];

            while (palavra != 0)
            {
                var bit = System.Numerics.BitOperations.TrailingZeroCount(palavra);

                yield return i * 64 + bit;

                palavra &= palavra - 1;
            }
        }
    }
}

/// <summary>
/// Os GRAFOS que separam os algoritmos, e por que eles sao assim.
///
/// Grafo sorteado nao separa quase nada: o numero de cliques maximais nele e
/// pequeno e qualquer busca termina rapido. O que separa sao os grafos ARMADOS, e
/// o mais importante deles e de 1965.
/// </summary>
public static class Grafos
{
    /// <summary>
    /// O GRAFO DE MOON e MOSER, que tem o MAIOR numero possivel de cliques
    /// maximais.
    ///
    /// Divida os vertices em grupos de TRES e ligue tudo o que esta em grupos
    /// diferentes, sem ligar nada dentro de um grupo. Toda clique maximal pega
    /// exatamente um vertice de cada grupo, e sao tres escolhas por grupo: o
    /// numero de cliques maximais e TRES ELEVADO A n sobre tres.
    ///
    /// Moon e Moser provaram em 1965 que nenhum grafo de n vertices tem mais que
    /// isso. Entao esse grafo nao e so um caso ruim: ele e O caso ruim, e nenhum
    /// algoritmo que liste todas as cliques maximais pode escapar dele.
    /// </summary>
    public static (Grafo Grafo, int MaiorClique, long CliquesMaximais) MoonMoser(int grupos)
    {
        var grafo = new Grafo(3 * grupos);

        for (var a = 0; a < 3 * grupos; a++)
            for (var b = a + 1; b < 3 * grupos; b++)
                if (a / 3 != b / 3) grafo.Ligar(a, b);

        var quantas = 1L;

        for (var i = 0; i < grupos; i++) quantas *= 3;

        return (grafo, grupos, quantas);
    }

    /// <summary>
    /// O GRAFO DE TURAN: grupos de tamanho igual, tudo ligado entre grupos.
    ///
    /// E a generalizacao do de Moon e Moser com grupos de qualquer tamanho. Com
    /// grupos de tres, os dois sao o mesmo grafo. A maior clique tem um vertice por
    /// grupo, sempre.
    /// </summary>
    public static (Grafo Grafo, int MaiorClique) Turan(int vertices, int grupos)
    {
        var grafo = new Grafo(vertices);

        for (var a = 0; a < vertices; a++)
            for (var b = a + 1; b < vertices; b++)
                if (a % grupos != b % grupos) grafo.Ligar(a, b);

        return (grafo, Math.Min(grupos, vertices));
    }

    public static Grafo Completo(int vertices)
    {
        var grafo = new Grafo(vertices);

        for (var a = 0; a < vertices; a++)
            for (var b = a + 1; b < vertices; b++) grafo.Ligar(a, b);

        return grafo;
    }

    public static Grafo Ciclo(int vertices)
    {
        var grafo = new Grafo(vertices);

        for (var v = 0; v < vertices; v++) grafo.Ligar(v, (v + 1) % vertices);

        return grafo;
    }

    /// <summary>Um grafo bipartido: a maior clique tem DOIS vertices, uma aresta.</summary>
    public static Grafo Bipartido(int esquerda, int direita, double chance, ulong semente)
    {
        var sorteio = new Sorteio(semente);
        var grafo = new Grafo(esquerda + direita);

        for (var i = 0; i < esquerda; i++)
            for (var j = 0; j < direita; j++)
                if (sorteio.Fracao() < chance) grafo.Ligar(i, esquerda + j);

        return grafo;
    }

    public static Grafo Sorteado(int vertices, double chance, ulong semente)
    {
        var sorteio = new Sorteio(semente);
        var grafo = new Grafo(vertices);

        for (var a = 0; a < vertices; a++)
            for (var b = a + 1; b < vertices; b++)
                if (sorteio.Fracao() < chance) grafo.Ligar(a, b);

        return grafo;
    }

    /// <summary>
    /// Um grafo com uma CLIQUE PLANTADA dentro de um sorteado.
    ///
    /// E o caso que separa "achar a maior" de "achar uma grande": a clique plantada
    /// esta la, e uma busca que para cedo nao a encontra.
    /// </summary>
    public static (Grafo Grafo, int Plantada) ComCliquePlantada(int vertices, int tamanho, double chance, ulong semente)
    {
        var grafo = Sorteado(vertices, chance, semente);
        var sorteio = new Sorteio(semente + 1_000);
        var escolhidos = new HashSet<int>();

        while (escolhidos.Count < tamanho) escolhidos.Add(sorteio.Ate(vertices));

        var lista = escolhidos.ToList();

        for (var i = 0; i < lista.Count; i++)
            for (var j = i + 1; j < lista.Count; j++) grafo.Ligar(lista[i], lista[j]);

        return (grafo, tamanho);
    }

    /// <summary>
    /// Os grafos PEQUENOS, onde a forca bruta sobre todos os subconjuntos cabe.
    ///
    /// Sao dois elevado ao numero de vertices, entao o juiz so existe ate umas duas
    /// dezenas. E e exatamente por isso que estes grafos precisam ser muitos e
    /// variados: eles sao o unico lugar onde a resposta e conferida contra a
    /// definicao e nao contra outro algoritmo.
    /// </summary>
    public static IEnumerable<(string Nome, Grafo Grafo)> Pequenos(int sorteados = 15)
    {
        yield return ("completo de 10", Completo(10));
        yield return ("ciclo de 12", Ciclo(12));
        yield return ("bipartido 6x6", Bipartido(6, 6, 0.5, 7));
        yield return ("Moon e Moser (4 grupos)", MoonMoser(4).Grafo);
        yield return ("Turan 12 em 4 grupos", Turan(12, 4).Grafo);
        yield return ("sem aresta", new Grafo(10));

        for (ulong semente = 1; semente <= (ulong)sorteados; semente++)
            yield return ($"sorteado 16 (30%) {semente}", Sorteado(16, 0.3, semente));

        for (ulong semente = 1; semente <= (ulong)sorteados; semente++)
            yield return ($"sorteado 16 (70%) {semente}", Sorteado(16, 0.7, semente));
    }

    public static IEnumerable<(string Nome, Grafo Grafo)> Grandes()
    {
        yield return ("Moon e Moser (8 grupos)", MoonMoser(8).Grafo);
        yield return ("Moon e Moser (10 grupos)", MoonMoser(10).Grafo);
        yield return ("completo de 60", Completo(60));
        yield return ("ciclo de 300", Ciclo(300));
        yield return ("sorteado 150 (10%)", Sorteado(150, 0.1, 11));
        yield return ("sorteado 150 (50%)", Sorteado(150, 0.5, 13));
        yield return ("sorteado 100 (80%)", Sorteado(100, 0.8, 17));
        yield return ("bipartido 80x80", Bipartido(80, 80, 0.3, 19));
    }
}
