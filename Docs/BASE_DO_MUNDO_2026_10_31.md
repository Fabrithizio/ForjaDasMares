# Base do mundo aprovada — 31/10/2026

Documento de direção narrativa. Separa decisões aprovadas de hipóteses e conteúdo efetivamente implementado. Substitui a premissa marítima anterior; nomes atuais são provisórios. Não altera arquivos de salvamento.

## Decisões aprovadas
- Outro mundo de fantasia, com diferentes povos; não é a Terra, nem um planeta obrigado a ter origem oceânica. Espécie e classe não precisam ser equivalentes.
- Um acontecimento rompeu a transmissão do passado e fez gerações perderem memórias. Idiomas, ofícios e costumes sobreviveram parcialmente; os personagens atuais têm vida anterior e não começam todos com amnésia pessoal.
- Existiram estruturas e conhecimentos muito mais avançados do que os atuais, mesmo antes da ruptura. Podem estar ligados ao acontecimento; a causa ainda NÃO foi definida.
- Vestígios únicos preservam evidências em objetos, lugares, registros, canções, comportamentos e mecanismos. Evidência não equivale automaticamente a explicação verdadeira.
- Exploração e comunidade descobrem histórias, missões, chefes e caminhos opcionais. Controles, atributos e funcionamento do equipamento devem ser claros.
- Não copiar nomes, símbolos, língua, objetos ou sequência narrativa de obras de referência. Não há pedras históricas universais como único meio de informação.
- Ambiente primeiro. Reaproveitar o que existe, PC prioritário, compatibilidade móvel, sem revisar agora combate e animações do personagem.

## Primeira base, em escala pequena
A cidade atual serve como assentamento provisório: moradores reaproveitam estruturas antigas, sem dominar sua fabricação. O jogador chega com uma origem e um motivo ainda a escolher; não será chamado de escolhido por padrão. Definir essa origem antes de substituir a abertura da campanha.

Primeira pergunta: por que habitantes que não sabem fabricar certa tecnologia continuam reparando e preservando um dispositivo antigo?
Primeira evidência: uma peça sem juntas aparentes, integrada por reparos mais recentes de pedra, mantém marcas luminosas. Isso demonstra diferença de conhecimento; não demonstra quem apagou memórias ou se o mecanismo é perigoso.

A antiga missão do sino segue funcionando como conteúdo legado. Não foi reescrita nem apresentada como implementação desta nova história. A migração será feita por trechos, preservando o diário existente e definindo IDs novos quando houver missões novas reais.

## Regra para cada descoberta futura
Registrar: observação acessível ao jogador; origem da evidência; interpretação possível; verdade interna quando decidida; o que a contradiz; consequência jogável. Nunca tratar uma hipótese do NPC como prova absoluta. Cada arco deve resolver uma pergunta antes de abrir outras.

## Questões ainda abertas
Nome do mundo/cidade/povos; data e alcance da ruptura; causa real e agentes envolvidos; antagonista inicial; origem do personagem; circunstâncias da chegada; aparência de cada povo. Não inventar respostas definitivas durante criação isolada de assets.

## Linguagem visual inicial
Fantasia habitada: madeira, tecido, pedra e vegetação com silhuetas legíveis. Tecnologia antiga: peças precisas, materiais pouco comuns, encaixes contínuos e poucos pontos luminosos. Reparos contemporâneos devem contrastar sem transformar a cidade em cenário de ficção científica moderno. Evitar tudo coberto de runas, neon ou partículas. Fenômenos maiores ficam para regiões futuras.

## Próximo kit de ambiente (planejado, não são assets existentes)
1. Painel antigo baixo com três marcas luminosas; 2. pilar tecnológico inteiro; 3. pilar quebrado; 4. arco com encaixe assimétrico; 5. placa de metal reparada com pedra; 6. raiz envolvendo estrutura; 7. arbusto em leque; 8. árvore de copa larga com pequenos pontos de luz; 9. luminária de oficina improvisada; 10. bancada de pesquisa com objetos separados; 11. marco de estrada reaproveitado; 12. cobertura de arquivo.
Antes de pedir gerações, comparar com o catálogo de materiais já organizado. Cada referência Meshy deve ter um objeto, fundo simples, sem textos, armações flutuantes ou acessórios fundidos; peças modulares com dimensões, pivôs e encaixes definidos. Texturas de chão são uma necessidade separada. Não gerar mais de cem objetos antes de validar o primeiro conjunto no jogo.

## Bloco 0.26.0
Uma estrutura examinável na cidade: AncientMemorySite. Geometria original provisória; três meshes combinadas e três materiais, sem novos scripts de atualização, luz dinâmica, partículas ou texturas. Clique/F reutilizam WorldClickTarget e TownHub.Speak. A inspeção não dá itens, não muda missão, não salva descobertas, não desbloqueia conteúdo global. Não existe ainda diário completo de descobertas ou migração narrativa da campanha.
