namespace Conde.Cliques;

/// <summary>
/// O ALGORITMO DE BRON e KERBOSCH, de 1973, que lista TODAS as cliques maximais.
///
/// Ele carrega tres conjuntos: a clique que esta sendo montada, os CANDIDATOS que
/// ainda podem entrar nela, e os JA USADOS, que sao os que foram tentados e
/// descartados neste ramo. Quando os dois ultimos ficam vazios, a clique montada e
/// maximal e sai.
///
/// O conjunto dos ja usados e o que todo mundo esquece, e sem ele o algoritmo
/// continua achando as cliques certas e as repete uma vez para cada ordem de
/// montagem. Ele nao existe para achar: existe para NAO REPETIR.
///
/// E por cima disso vem o PIVO, de 1973 tambem: em vez de tentar todo candidato,
/// escolha um vertice e tente so os candidatos que NAO sao vizinhos dele. Toda
/// clique maximal ou contem o pivo ou exclui algum vizinho dele, entao nenhuma se
/// perde, e o ramo encolhe muito. A tabela do medidor diz quanto.
/// </summary>
public static class BronKerbosch
{
    /// <param name="Quantas">quantas cliques maximais</param>
    /// <param name="MaiorClique">o tamanho da maior delas</param>
    /// <param name="Nos">quantas chamadas a busca fez</param>
    public record Resultado(long Quantas, int MaiorClique, long Nos);

    public static Resultado Listar(Grafo grafo, bool comPivo = true, List<int[]>? guardar = null)
    {
        var estado = new Estado(grafo, comPivo, guardar);

        estado.Buscar([], grafo.TodosOsVertices(), new ulong[grafo.Palavras]);

        return new Resultado(estado.Quantas, estado.MaiorClique, estado.Nos);
    }

    /// <summary>Todas as cliques maximais, como listas de vertices.</summary>
    public static List<int[]> Todas(Grafo grafo, bool comPivo = true)
    {
        var lista = new List<int[]>();

        Listar(grafo, comPivo, lista);

        return lista;
    }

    private sealed class Estado(Grafo grafo, bool comPivo, List<int[]>? guardar)
    {
        public long Quantas { get; private set; }

        public int MaiorClique { get; private set; }

        public long Nos { get; private set; }

        /// <summary>
        /// A recursao e funda no maximo o tamanho da maior clique, que neste
        /// repositorio nao passa de algumas dezenas.
        ///
        /// Nao e o caso dos repositorios vizinhos em que a profundidade e o numero
        /// de vertices e a pilha do macOS estourou na integracao continua: aqui o
        /// fundo e a resposta, e a resposta e pequena por natureza do problema.
        /// </summary>
        public void Buscar(List<int> montada, ulong[] candidatos, ulong[] usados)
        {
            Nos++;

            if (Bits.Vazio(candidatos) && Bits.Vazio(usados))
            {
                Quantas++;
                MaiorClique = Math.Max(MaiorClique, montada.Count);
                guardar?.Add([.. montada.Order()]);

                return;
            }

            var tentar = comPivo ? SemOsVizinhosDoPivo(candidatos, usados) : Bits.Copia(candidatos);

            foreach (var v in Bits.Membros(tentar))
            {
                if (!Bits.Tem(candidatos, v)) continue;

                var vizinhos = grafo.De(v);

                montada.Add(v);
                Buscar(montada, Bits.E(candidatos, vizinhos), Bits.E(usados, vizinhos));
                montada.RemoveAt(montada.Count - 1);

                Bits.Apagar(candidatos, v);
                Bits.Acender(usados, v);
            }
        }

        /// <summary>
        /// O PIVO: o vertice com mais vizinhos entre os candidatos.
        ///
        /// Escolher o de MAIS vizinhos e o que faz a economia, porque o ramo tenta
        /// so os candidatos de fora da vizinhanca dele. Um pivo qualquer ja estaria
        /// correto e economizaria pouco: a diferenca entre "qualquer" e "o melhor"
        /// esta na tabela do medidor.
        /// </summary>
        private ulong[] SemOsVizinhosDoPivo(ulong[] candidatos, ulong[] usados)
        {
            var pivo = -1;
            var melhor = -1;

            foreach (var u in Bits.Membros(candidatos).Concat(Bits.Membros(usados)))
            {
                var quantos = Bits.Quantos(Bits.E(candidatos, grafo.De(u)));

                if (quantos <= melhor) continue;

                melhor = quantos;
                pivo = u;
            }

            if (pivo < 0) return Bits.Copia(candidatos);

            var vizinhosDoPivo = grafo.De(pivo);
            var tentar = new ulong[candidatos.Length];

            for (var i = 0; i < candidatos.Length; i++) tentar[i] = candidatos[i] & ~vizinhosDoPivo[i];

            return tentar;
        }
    }

    /// <summary>
    /// A MAIOR CLIQUE, pelo mesmo algoritmo: a maior das maximais.
    ///
    /// Toda clique maxima e maximal, entao listar todas as maximais e pegar a maior
    /// resolve. Nao e o jeito mais rapido de achar so a maior, e e o jeito que
    /// torna as duas respostas a mesma conta, que e o que se quer quando o assunto
    /// e conferir uma contra a outra.
    /// </summary>
    public static int MaiorClique(Grafo grafo) => Listar(grafo).MaiorClique;

    /// <summary>Um conjunto de vertices e uma clique quando todos sao vizinhos entre si.</summary>
    public static bool EhClique(Grafo grafo, IReadOnlyList<int> vertices)
    {
        for (var i = 0; i < vertices.Count; i++)
            for (var j = i + 1; j < vertices.Count; j++)
                if (!grafo.Vizinhos(vertices[i], vertices[j])) return false;

        return true;
    }

    /// <summary>
    /// E ela e MAXIMAL quando nenhum vertice de fora pode entrar.
    ///
    /// Maximal nao e maxima, e a diferenca e o repositorio inteiro: num grafo de
    /// Moon e Moser com trinta vertices ha quase sessenta mil cliques maximais, e
    /// todas elas tem exatamente o mesmo tamanho.
    /// </summary>
    public static bool EhMaximal(Grafo grafo, IReadOnlyList<int> vertices)
    {
        if (!EhClique(grafo, vertices)) return false;

        for (var v = 0; v < grafo.Vertices; v++)
        {
            if (vertices.Contains(v)) continue;
            if (vertices.All(u => grafo.Vizinhos(v, u))) return false;
        }

        return true;
    }
}
