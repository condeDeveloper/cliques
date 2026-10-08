using Conde.Cliques;

var pedido = args.Length > 0 ? args[0] : "tudo";
var tudo = pedido == "tudo";

if (tudo || pedido == "definicao") Definicao();
if (tudo || pedido == "pivo") Pivos();
if (tudo || pedido == "moonmoser") MoonMosers();
if (tudo || pedido == "densidade") Densidades();
if (tudo || pedido == "grande") Grandes();

static void Definicao()
{
    Console.WriteLine();
    Console.WriteLine("  O ALGORITMO CONTRA A DEFINICAO");
    Console.WriteLine();
    Console.WriteLine("  grafo                      vertices   arestas   maior (forca bruta)   maior (algoritmo)   maior (complemento)   cliques maximais   as listas batem");

    foreach (var l in Medidas.ContraADefinicao())
        Console.WriteLine($"  {l.Grafo,-26} {l.Vertices,8} {l.Arestas,9} {l.MaiorPelaForcaBruta,21} {l.MaiorPeloAlgoritmo,19} {l.PeloComplemento,21} {l.Maximais,18:N0} {Marca(l.AsListasBatem),17}");

    Console.WriteLine();
    Console.WriteLine("  as tres colunas de maior vem de lugares diferentes, e a ultima nem fala de");
    Console.WriteLine("  clique: ela e o maior conjunto SEM ARESTA do complemento");
    Console.WriteLine();
}

static void Pivos()
{
    Console.WriteLine();
    Console.WriteLine("  O PIVO CONTRA A FALTA DELE, EM NOS DA BUSCA");
    Console.WriteLine();
    Console.WriteLine("  grafo                      vertices   cliques maximais        sem pivo        com pivo   quantas vezes menos");

    foreach (var l in Medidas.OPivo())
        Console.WriteLine($"  {l.Grafo,-26} {l.Vertices,8:N0} {l.Maximais,18:N0} {l.SemPivo,15:N0} {l.ComPivo,15:N0} {l.QuantasVezesMenos,21:F1}");

    Console.WriteLine();
    Console.WriteLine("  as duas versoes acham exatamente as mesmas cliques: o pivo nao muda a");
    Console.WriteLine("  resposta, muda quanto do espaco de busca e sequer gerado");
    Console.WriteLine();
}

static void MoonMosers()
{
    Console.WriteLine();
    Console.WriteLine("  O GRAFO DE MOON E MOSER CONTRA O LIMITE DELES");
    Console.WriteLine();
    Console.WriteLine("  grupos   vertices   o limite (3^(n/3))   cliques maximais medidas   maior clique   nos da busca");

    foreach (var l in Medidas.MoonMoser())
        Console.WriteLine($"  {l.Grupos,6} {l.Vertices,10} {l.OLimite,20:N0} {l.Medidas,26:N0} {l.MaiorClique,14} {l.Nos,14:N0}");

    Console.WriteLine();
    Console.WriteLine("  o limite e ATINGIDO em toda linha: este grafo nao e um caso ruim entre");
    Console.WriteLine("  outros, ele e O caso ruim, e nenhum algoritmo escapa dele");
    Console.WriteLine();
}

static void Densidades()
{
    Console.WriteLine();
    Console.WriteLine("  QUANTAS CLIQUES MAXIMAIS UM GRAFO SORTEADO DE 60 VERTICES TEM");
    Console.WriteLine();
    Console.WriteLine("  chance   arestas   cliques maximais   maior clique   nos da busca   nos por clique");

    foreach (var l in Medidas.Densidade())
        Console.WriteLine($"  {l.Chance,6:F2} {l.Arestas,9:N0} {l.Maximais,18:N0} {l.MaiorClique,14} {l.Nos,14:N0} {l.NosPorClique,16:F2}");

    Console.WriteLine();
    Console.WriteLine("  a curva nao e monotona: com poucas arestas toda clique maximal e uma aresta");
    Console.WriteLine("  sozinha, e com muitas o grafo inteiro vira quase uma clique so");
    Console.WriteLine();
}

static void Grandes()
{
    Console.WriteLine();
    Console.WriteLine("  OS GRAFOS GRANDES, ONDE A FORCA BRUTA NAO CHEGA");
    Console.WriteLine();
    Console.WriteLine("  grafo                      vertices   arestas   cliques maximais   maior clique achada   nos da busca");

    foreach (var l in Medidas.Grandes())
        Console.WriteLine($"  {l.Grafo,-26} {l.Vertices,8:N0} {l.Arestas,9:N0} {l.Maximais,18:N0} {l.MaiorClique,21} {l.Nos,14:N0}");

    Console.WriteLine();
    Console.WriteLine("  a coluna se chama \"maior clique ACHADA\" de proposito: nesses tamanhos este");
    Console.WriteLine("  repositorio nao tem como sustentar que ela e a maior que existe");
    Console.WriteLine();
}

static string Marca(bool valor) => valor ? "sim" : "NAO";
