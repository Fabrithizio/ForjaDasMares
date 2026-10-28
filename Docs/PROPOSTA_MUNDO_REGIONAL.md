# Mundo regional compartilhado — proposta de arquitetura

Status: planejamento e auditoria, não implementado. As masmorras/salas atuais ficam preservadas como desafios privados. Não foram alterados scripts, cenas, prefabs ou builds nesta análise.

Direção solicitada: RPG de ação PC/Android, história contínua, regiões públicas com limite-alvo de 20 jogadores por instância e locais privados para grupos de quatro. Inimigos e recursos do mundo compartilham estado dentro da instância e reaparecem sob controle do servidor. A mesma região pode ter várias cópias simultâneas. 1000 jogadores numa única região exigiriam no mínimo 50 instâncias de capacidade 20; reservas, grupos e margem operacional podem exigir mais. Isso não equivale a 50 mapas diferentes, 50 máquinas ou 1000 jogadores sincronizados no celular.

## Limites atuais comprovados
CoopDirector possui arrays de atores e frames fixos de quatro posições e verificações de slot <4. CoopCloud cria SessionOptions.MaxPlayers=4 com Relay. O modelo é host-jogador, não um servidor público persistente. Não basta alterar o limite da sala para 20. Saves locais e perfis de host não podem virar a fonte confiável de uma economia pública.
Pacotes observados: Unity 6000.3.26f1; URP17.3.0; NGO2.13.3; Transport2.7.4; Multiplayer Services2.4.0; AI Navigation2.0.16. Nenhuma dependência foi atualizada.

## Proposta
Servidor dedicado controla cada instância: movimento validado, IA, dano, mortes, geração/respawn, interações, distribuição de recompensas. Serviço de entrada localiza uma instância da região, reserva vagas para o grupo e inicia outra quando necessário. Transição entre regiões deve transferir o personagem com validação do backend e proteção contra duplicação. Cadastro/autenticação e progresso permanente ficam em serviços do servidor; o celular não concede ouro/itens.

Priorizar grupo inteiro numa mesma instância; reservar vagas e só confirmar viagem quando houver espaço. Jogador solo ocupa vaga disponível. O grupo pode entrar numa masmorra privada de quatro e retornar à região pública. Saída de um jogador não encerra a região. Instâncias vazias podem ser encerradas após período de tolerância, preservando o progresso permanente e as regras de eventos.

Inimigo morto desaparece para todos naquela instância e tem respawn controlado, preferencialmente em acampamentos/portais com aviso coerente. Política sugerida, ainda a decidir: monstro compartilhado; XP e loot individuais para participantes elegíveis/grupo, sem depender do último golpe. Coleta de recursos pode usar nós compartilhados com cooldown; regras anti-abuso e participação precisam ser definidas antes da economia. Inimigos de missão podem precisar de alternativas para não bloquear progressão.

## Construção do mundo
Escolha proposta: híbrido. Terrain em blocos para áreas externas, relevo e caminhos naturais; malhas modulares para ruínas, pisos trabalhados, pontes, penhascos e cavernas. Terrain sozinho não fornece tetos/volumes de cavernas. Não montar o mundo numa placa gigantesca, nem usar cada ZIP como um mapa inteiro. Regiões desenhadas à mão com pontos de interesse, carregamento por setores e transições entre regiões. Navegação/colisões separadas da malha decorativa, limites e travessias claramente definidos.

Começo proposto: uma região piloto, provisoriamente Costa das Ruínas, com refúgio seguro, rota de história, três zonas de combate, atalho, segredo e entrada para os desafios existentes. Primeiro validar espaço, câmera e combate com poucos clientes; depois ensaio com 20 conexões reais/simuladas e medições. Não anunciar 20 por região ou 1000 simultâneos como capacidade testada.

## Auditoria dos sete ZIPs do usuário
Origem: Desktop/imagens refenteica/assets gerado por ia/asstes. Leitura direta do FBX binário e dimensões PNG, sem alterar fontes ou importá-las no jogo. Todos têm uma geometria, um material, quatro texturas 2048x2048 (base/metallic/normal/roughness) e nenhum AnimationStack.
- Ancient Flame Bastion: 6359 vértices, 11978 triângulos.
- Emberstone Citadel Bridge: 6574 vértices, 10950 triângulos.
- Emberwatch Gate: 6459 vértices, 11634 triângulos.
- Molten Spire Citadel: 6340 vértices, 12681 triângulos.
- Mossbound Relic: 6199 vértices, 11351 triângulos.
- Mossbound Ruin Gate: 6213 vértices, 11153 triângulos.
- Sunfall Ruin: 6331 vértices, 12290 triângulos.
ZIPs: cerca de 21–24 MB cada, não é medida de memória em execução. São candidatos a marcos/ruínas/portais/cenário; não foi validada a limpeza topológica, qualidade de UV, escala, passagem em portais ou colisão na Unity. A pasta imagem contém referências PNG; imagem de monstro não é modelo animado. Nenhum monstro 3D foi encontrado nesses sete ZIPs.

## Otimização proposta, a medir
Criar variantes LOD apenas onde a perda de forma for aceitável; aproveitar materiais únicos e testar instanciamento. Texturas menores para objetos pequenos, mipmaps e compressão por plataforma; converter roughness para smoothness/empacotar canais para o material URP. Colisores simples e piso navegável separado; nunca depender do MeshCollider detalhado de toda decoração. Não reduzir malhas automaticamente sem comparar silhueta e UV.
Carregar setores próximos; limitar animações/partículas/luzes/sombras por distância e perfil gráfico. IA ativa por proximidade, pooling e atualizações distribuídas. Rede replica entidades relevantes pela posição do jogador, sem enviar a região inteira a cada frame. Não desligar indiscriminadamente entidades só porque estão fora da câmera de um cliente: outros jogadores podem estar próximos.
Metas iniciais a validar: PC 60fps e Android modesto 30fps estáveis, com orçamento de tempo de quadro e memória medido no dispositivo. Não são resultados atuais. Testar 1, 4, 10 e 20 jogadores e combates concentrados; medir CPU, GPU, memória, física, carregamento e banda antes de expandir.

## Próximos blocos
1. Especificar mapa piloto e criar estudo visual/colisão com os modelos numa cena nova, sem substituir desafios.
2. Separar sessão de grupo e instância regional, remover pressupostos de quatro apenas nos sistemas que precisem disso; manter party privada de quatro.
3. Prototipar servidor dedicado local e acesso à região, primeiro com poucos clientes.
4. Respawn compartilhado, participação/recompensas e transferência segura de região/perfil.
5. Testes de carga e dispositivo; só então escolher hospedagem e dimensionar/cotar escala pública.
Sem compra, contratação de infraestrutura, atualização de pacotes ou implantação realizada.

Fontes consultadas: https://docs.unity.com/en-us/mps-sdk/networking/relay-servers ; https://docs.unity.com/en-us/mps-sdk/sessions-overview ; https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/dedicated-server/get-started/optimizations ; https://docs.unity.com/en-us/engine/6000.5/manual/creating-environments/script-terrain/terrain-height/terrain-basic-sculpting/terrain-paint-holes .
