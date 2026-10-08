using Conde.Cliques;
using Xunit;

namespace Conde.Cliques.Testes;

public class BronKerboschTestes
{
    /// <summary>
    /// O algoritmo acha exatamente as MESMAS CLIQUES que a forca bruta, e nao so a
    /// mesma quantidade.
    ///
    /// A distincao importa: duas listas com a mesma contagem e conteudos diferentes
    /// seriam o erro mais facil de nao notar, porque a unica coluna que a maioria
    /// olha e a da contagem.
    /// </summary>
    [Fact]
    public void OAlgoritmoAchaAsMesmasCliquesQueAForcaBruta()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos(10))
        {
            if (grafo.Vertices > 18) continue;

            Assert.True(
                ForcaBruta.MesmasCliques(BronKerbosch.Todas(grafo), ForcaBruta.TodasAsMaximais(grafo)),
                nome);
        }
    }

    /// <summary>
    /// E as DUAS VERSOES do algoritmo, com e sem pivo, acham as mesmas cliques.
    ///
    /// O pivo nao muda a resposta: ele muda quanto do espaco de busca e sequer
    /// gerado. Se ele mudasse a resposta, seria uma poda errada e nao uma poda.
    /// </summary>
    [Fact]
    public void ComPivoESemPivoAchamAsMesmasCliques()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos(10))
            Assert.True(
                ForcaBruta.MesmasCliques(BronKerbosch.Todas(grafo, true), BronKerbosch.Todas(grafo, false)),
                nome);
    }

    /// <summary>
    /// Toda clique devolvida E uma clique E e MAXIMAL.
    ///
    /// As duas metades importam. So a primeira aceitaria uma lista de arestas
    /// soltas; so a segunda nao faz sentido sem a primeira.
    /// </summary>
    [Fact]
    public void TodaCliqueDevolvidaEhUmaCliqueMaximal()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos(10))
            foreach (var clique in BronKerbosch.Todas(grafo))
            {
                Assert.True(BronKerbosch.EhClique(grafo, clique), $"{nome}: {string.Join(",", clique)}");
                Assert.True(BronKerbosch.EhMaximal(grafo, clique), $"{nome}: {string.Join(",", clique)}");
            }
    }

    /// <summary>
    /// E nos grafos GRANDES, onde a forca bruta nao chega, as duas conferencias
    /// continuam valendo.
    ///
    /// E o unico tipo de afirmacao que da para sustentar la: nao "esta e a maior
    /// clique", que exigiria o juiz, mas "esta e uma clique e nao da para aumentar
    /// ela", que e conferivel direto.
    /// </summary>
    [Fact]
    public void NosGrafosGrandesAsCliquesContinuamSendoCliquesMaximais()
    {
        foreach (var (nome, grafo) in new (string, Grafo)[]
        {
            ("sorteado 150 (10%)", Grafos.Sorteado(150, 0.1, 11)),
            ("ciclo de 300", Grafos.Ciclo(300)),
            ("bipartido 80x80", Grafos.Bipartido(80, 80, 0.3, 19)),
            ("Moon e Moser (8 grupos)", Grafos.MoonMoser(8).Grafo),
        })
            foreach (var clique in BronKerbosch.Todas(grafo).Take(500))
            {
                Assert.True(BronKerbosch.EhClique(grafo, clique), nome);
                Assert.True(BronKerbosch.EhMaximal(grafo, clique), nome);
            }
    }

    /// <summary>
    /// O ACHADO PRINCIPAL: no grafo completo, o pivo corta de UM MILHAO de nos para
    /// VINTE E UM.
    ///
    /// O grafo completo de vinte vertices tem UMA clique maximal, que e ele
    /// inteiro. Sem pivo, a busca gera todos os dois elevado a vinte subconjuntos
    /// antes de descobrir isso, porque todo subconjunto e uma clique e nenhum deles
    /// e maximal ate o ultimo.
    ///
    /// Com pivo, o primeiro vertice escolhido ja e vizinho de todo mundo, entao nao
    /// sobra candidato nenhum para tentar e o ramo e um so.
    /// </summary>
    [Fact]
    public void NoGrafoCompletoOPivoCortaDeUmMilhaoParaVinteEUm()
    {
        var grafo = Grafos.Completo(20);

        var sem = BronKerbosch.Listar(grafo, false);
        var com = BronKerbosch.Listar(grafo, true);

        Assert.Equal(1, sem.Quantas);
        Assert.Equal(1, com.Quantas);
        Assert.Equal(1_048_576, sem.Nos);
        Assert.Equal(21, com.Nos);
    }

    /// <summary>
    /// O ACHADO QUE ME CORRIGIU: o pivo NAO ajuda sempre, e no ciclo ele nao ajuda
    /// nada.
    ///
    /// Eu esperava que ele cortasse em todo grafo, porque ele corta tanto no
    /// completo. Num ciclo de quarenta vertices sao 81 nos sem pivo e 79 com, e os
    /// dois a mais do calculo do pivo nem aparecem na conta.
    ///
    /// Faz sentido depois de ver: o pivo so economiza quando ele tem MUITOS
    /// vizinhos entre os candidatos, e num ciclo todo vertice tem dois. Ele nao tem
    /// o que podar.
    /// </summary>
    [Fact]
    public void NoCicloOPivoNaoAjudaNada()
    {
        var grafo = Grafos.Ciclo(40);

        var sem = BronKerbosch.Listar(grafo, false);
        var com = BronKerbosch.Listar(grafo, true);

        Assert.Equal(40, sem.Quantas);
        Assert.Equal(40, com.Quantas);
        Assert.InRange((double)sem.Nos / com.Nos, 0.9, 1.2);
    }

    /// <summary>
    /// E em grafo DENSO ele volta a cortar muito: 168 vezes menos nos num sorteado
    /// com oitenta por cento das arestas.
    /// </summary>
    [Fact]
    public void EmGrafoDensoOPivoVoltaACortarMuito()
    {
        var grafo = Grafos.Sorteado(60, 0.8, 17);

        var sem = BronKerbosch.Listar(grafo, false);
        var com = BronKerbosch.Listar(grafo, true);

        Assert.Equal(sem.Quantas, com.Quantas);
        Assert.True(sem.Nos > 100 * com.Nos, $"sem {sem.Nos}, com {com.Nos}");
    }

    /// <summary>
    /// A conferencia de MAXIMAL recusa uma clique que da para aumentar, e a de
    /// CLIQUE recusa um conjunto com dois nao vizinhos.
    /// </summary>
    [Fact]
    public void AsConferenciasRecusamOQueDevem()
    {
        var grafo = Grafos.Completo(5);

        Assert.True(BronKerbosch.EhClique(grafo, [0, 1, 2]));
        Assert.False(BronKerbosch.EhMaximal(grafo, [0, 1, 2]));
        Assert.True(BronKerbosch.EhMaximal(grafo, [0, 1, 2, 3, 4]));

        var ciclo = Grafos.Ciclo(5);

        Assert.False(BronKerbosch.EhClique(ciclo, [0, 2]));
        Assert.True(BronKerbosch.EhClique(ciclo, [0, 1]));
        Assert.True(BronKerbosch.EhMaximal(ciclo, [0, 1]));
    }

    /// <summary>
    /// A MAIOR CLIQUE que o algoritmo devolve e a mesma que a forca bruta acha, em
    /// todo grafo pequeno.
    /// </summary>
    [Fact]
    public void AMaiorCliqueBateComAForcaBruta()
    {
        foreach (var (nome, grafo) in Grafos.Pequenos(15))
            Assert.Equal(ForcaBruta.Maior(grafo).Tamanho, BronKerbosch.MaiorClique(grafo));
    }

    [Fact]
    public void OGrafoSemArestaTemUmaCliqueMaximalPorVertice()
    {
        var grafo = new Grafo(10);
        var r = BronKerbosch.Listar(grafo);

        Assert.Equal(10, r.Quantas);
        Assert.Equal(1, r.MaiorClique);
    }
}
