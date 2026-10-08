# cliques

Cliques maximais do zero em C# e .NET 8: **Bron e Kerbosch**, de 1973, com e sem
pivô. O juiz é a **definição** — uma clique é um grupo de vértices todos vizinhos
entre si, a maior é a maior, e saber que ela é a maior exige olhar todos os
subconjuntos.

E há um segundo juiz, que não fala de clique nenhuma: a maior clique de um grafo
é o maior conjunto **sem aresta** do complemento dele.

```
$ dotnet medidor.dll definicao

  grafo                      vertices   arestas   maior (forca bruta)   maior (algoritmo)   maior (complemento)   cliques maximais   as listas batem
  completo de 10                   10        45                    10                  10                    10                  1               sim
  ciclo de 12                      12        12                     2                   2                     2                 12               sim
  bipartido 6x6                    12        20                     2                   2                     2                 20               sim
  Moon e Moser (4 grupos)          12        54                     4                   4                     4                 81               sim
  Turan 12 em 4 grupos             12        54                     4                   4                     4                 81               sim
  sem aresta                       10         0                     1                   1                     1                 10               sim
```

A última coluna é a que importa mais: o algoritmo precisa achar o **mesmo
conjunto** de cliques que a força bruta, e não a mesma quantidade. Duas listas com
a mesma contagem e conteúdos diferentes seriam o erro mais fácil de não notar,
porque a única coluna que a maioria olha é a da contagem.

## O caso ruim tem nome e data

```
$ dotnet medidor.dll moonmoser

  grupos   vertices   o limite (3^(n/3))   cliques maximais medidas   maior clique   nos da busca
       1          3                    3                          3              1              4
       4         12                   81                         81              4            121
       6         18                  729                        729              6          1,093
       8         24                6,561                      6,561              8          9,841
      10         30               59,049                     59,049             10         88,573
```

Divida os vértices em grupos de **três** e ligue tudo o que está em grupos
diferentes. Toda clique maximal pega exatamente um vértice de cada grupo, e são
três escolhas por grupo: **3^(n/3)** cliques maximais.

Moon e Moser provaram em 1965 que nenhum grafo de n vértices tem mais que isso.
Então esse grafo não é um caso ruim entre outros: ele é **o** caso ruim, e nenhum
algoritmo que liste todas as cliques maximais pode escapar dele. O limite é
atingido em toda linha da tabela.

## O pivô corta cinquenta mil vezes, ou nada

O pivô é uma linha: em vez de tentar todo candidato, escolha um vértice e tente
só os candidatos que **não** são vizinhos dele. Toda clique maximal ou contém o
pivô ou exclui algum vizinho dele, então nenhuma se perde.

```
$ dotnet medidor.dll pivo

  grafo                      vertices   cliques maximais        sem pivo        com pivo   quantas vezes menos
  completo de 20                   20                  1       1,048,576              21               49932.2
  ciclo de 40                      40                 40              81              79                   1.0
  bipartido 20x20                  40                172             213             200                   1.1
  Moon e Moser (5 grupos)          15                243           1,024             364                   2.8
  Moon e Moser (8 grupos)          24              6,561          65,536           9,841                   6.7
  sorteado 60 (20%)                60                241             814             481                   1.7
  sorteado 60 (50%)                60              2,223          26,511           5,216                   5.1
  sorteado 60 (80%)                60             56,437      23,177,908         137,455                 168.6
```

No grafo completo de vinte vértices, a busca sem pivô gera **um milhão** de nós
antes de descobrir que há uma clique maximal só, porque todo subconjunto é uma
clique e nenhum é maximal até o último. Com pivô, o primeiro vértice escolhido já
é vizinho de todo mundo, não sobra candidato nenhum e o ramo é um só: vinte e um
nós.

E no ciclo ele não corta **nada**.

## Quantas cliques um grafo tem

```
$ dotnet medidor.dll densidade

  chance   arestas   cliques maximais   maior clique   nos da busca   nos por clique
    0.10       162                117              4            206             1.76
    0.30       538                464              5            994             2.14
    0.50       903              1,941              9          4,584             2.36
    0.70     1,258             15,811             12         37,938             2.40
    0.80     1,438             71,717             17        177,030             2.47
    0.90     1,606            541,136             23      1,409,132             2.60
    0.95     1,690            148,520             35        394,764             2.66
    0.99     1,751              9,216             46         23,862             2.59
    1.00     1,770                  1             60             61            61.00
```

Sessenta vértices. O número de cliques maximais sobe até meio milhão com noventa
por cento das arestas e **desaba para uma** quando o grafo fecha. O tamanho da
maior, enquanto isso, só cresce.

São duas coisas diferentes que o mesmo algoritmo entrega juntas, e elas não se
comportam parecido.

## Como rodar

```
dotnet test testes/Cliques.Testes/Cliques.Testes.csproj
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tudo
```

Os comandos do medidor são `definicao`, `moonmoser`, `pivo`, `densidade`,
`grande` e `tudo`.

Nada aqui é medido em segundos: a unidade é **nós da busca**, porque segundo
depende de máquina e de quem mais está rodando, e nó é o mesmo número no Linux,
no Windows e no macOS.

## O que as medidas me corrigiram

**O pivô não ajuda sempre, e no ciclo ele não ajuda nada.** Eu esperava que ele
cortasse em todo grafo, porque ele corta tanto no completo que a tabela parece
uma piada. Num ciclo de quarenta vértices são 81 nós sem pivô e 79 com, e o
cálculo do pivô em si nem aparece na conta.

Faz sentido depois de ver: o pivô só economiza quando ele tem **muitos** vizinhos
entre os candidatos, e num ciclo todo vértice tem dois. Não há o que podar. A
economia dele não é uma propriedade do algoritmo, é uma propriedade do grafo.

**O conjunto dos já usados não existe para achar: existe para não repetir.** Tirá-lo
do algoritmo não perde nenhuma clique — ele continua achando todas as certas, e
repete cada uma uma vez para cada ordem de montagem possível. O erro só aparece na
contagem, e a resposta "está certa" o tempo todo.

**A tabela de densidade me desmentiu enquanto eu a escrevia.** Eu tinha escrito no
comentário que a curva não é monótona, com o pico no meio, porque é isso que a
intuição diz: poucas arestas, poucas cliques; muitas arestas, uma clique só.

Com as densidades até 0,9 a curva só subia, e o texto estava errado. O pico está
em **0,9**, não no meio, e para vê-lo é preciso medir 0,95 e 0,99 — que é o que a
tabela faz agora.

**"Maior clique" e "maior clique achada" são colunas diferentes.** Nos grafos
grandes não há juiz, e escrever "maior clique" ali seria publicar como resultado
uma coisa que este repositório não tem como sustentar. O que dá para sustentar
é que ela **é** uma clique e que não dá para aumentá-la, e as duas coisas são
conferidas em teste nos grafos grandes também.

## Licença

MIT.
