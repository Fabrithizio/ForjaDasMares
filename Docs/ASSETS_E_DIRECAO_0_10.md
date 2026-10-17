# Assets e direção — campanha 0.10

## O que preciso de você

Nenhuma compra ou conta nova é necessária para esta iteração. Foram produzidos modelos e ícones próprios no Blender, e sons sintetizados com código. O projeto pode avançar com esses recursos. Para direção artística final, será útil escolher depois entre proporções mais cartunescas ou mais realistas e confirmar o tema dos biomas; não é uma dependência para testar agora.

## Conjunto original produzido

Fonte editável: ArtSource/ExpeditionSet/ExpeditionSet.blend e create_expedition_set.py. Exportações e ícones: Assets/ForjaDasMares/Art/ExpeditionSet. Sem arquivos, modelos ou texturas copiados de Dungeon Rampage. Ícones são renders dos objetos originais, não imagens tiradas da referência.

| Uso | Arquivos/modelos | Estado |
|---|---|---|
| Suprimentos e obstáculos | supply, crate, barrel, rock | Modelos low poly, suprimento identificado por marca verde |
| Objetivos e cura | seal, well | Cristal e fonte originais; ações ligadas aos sistemas reais |
| Ambientação | bush, mushrooms, banner | Modelos iniciais; variantes por bioma |
| Inimigos comuns | raider, guardian, archer, unstable, summoner | Silhuetas com armas/equipamentos distintos; movimento procedural dos membros |
| Elites | elite_warden, elite_shaman, elite_reaver | Silhuetas próprias; padrões telegrafados reutilizam combate real |
| Inventário | Sword, Axe, Hammer, Bow, Daggers, Staff, Grimoire, Shield, MagicWeapon, Armor, Accessory, Artifact |12 PNG transparentes128×128, compartilhados por categoria; não é ícone exclusivo de cada item |
| Sons | Hit, Crate, Heal, Loot |4 arquivos WAV mono sintetizados; placeholders originais |

Total:17 modelosFBX,12 íconesPNG e4sonsWAV. Não há rig humanoide nem clips novos nessa coleção. Brasa mantém o modelo e os clips existentes. EnemyModelMotion é movimento procedural simples, não animação artística final. Sem música, locução ou efeitos sonoros de qualidade final.

## Lista priorizada para arte final

| Prioridade | Recurso desejado | Especificação inicial | Alternativa atual |
|---|---|---|---|
|1|Quatro heróis coesos|1 modelo por herói, rig, sockets de arma/escudo; idle, caminhada, corrida,3 ataques, habilidade, carga, esquiva, hit, morte e vitória. Meta inicial3–8mil triângulos por herói; revisar com profiling.|Brasa com clips próprios; demais visuais provisórios|
|1|Animações dos inimigos|Cinco bases e três elites;4–8 clips cada, ataques sincronizados com preparação/impacto.|Movimento procedural; telegraph funcional|
|1|Texturas de cenário dos3 biomas|Atlas de cores/materiais e tiles legíveis de cima;512–2048px conforme medição, URP.|Materiais planos próprios|
|1|Sons de combate|Variações de espada, arco, magia, escudo, ataque inimigo, preparo de chefe, passos, impacto e morte.|4 sons sintetizados e avisos existentes|
|2|Módulos de mapas|Paredes externas, curvas, pedras, árvores/raízes, pilares, ruínas, lava/forja, pontes; colisores simples;20–30 peças por bioma.|Geometria existente+9 props novos|
|2|Chefes finais|Silhueta por bioma, rig, ao menos3 ataques legíveis e animações de preparação/recuperação.|Chefe e padrões existentes com modelos provisórios|
|2|VFX próprios|Corte em arco, flecha, impacto, explosão, gelo/raiz, proteção, cura, raro/épico, fumaça de perigo.|Zonas geométricas, flash e números reais|
|2|Ícones por item e habilidade|128–256px, transparência, leitura em64px; linguagem visual consistente.|12 imagens por categoria|
|3|Áudio ambiente e música|3 loops ambientais e2–4 faixas com transição exploração/combate.|Não produzido nesta entrega|

Se você trouxer packs gratuitos, precisamos do link e licença permitindo uso no jogo comercial. Não basta estarem disponíveis para baixar. Evitar modelos com shaders exclusivos de HDRP, dependências de projetos inteiros ou milhares de materiais. Preferir FBX/GLB com fonte, texturas e licença guardada. Nenhum pack externo foi adquirido ou incluído nesta etapa.

## Referência de mecânicas

Foi consultada a página oficial de Dungeon Rampage na Steam: https://store.steampowered.com/app/3053950/Dungeon_Rampage . Ela descreve combate cooperativo, masmorras, armas e habilidades. A adaptação deste projeto usa princípios gerais de hordas, destruição, saque e variedade, com dados/arte/nomes próprios. Não foi comprovada uma contagem exata de inimigos simultâneos da referência. A campanha aqui usa mais de100 inimigos acumulados nos encontros difíceis, com limite simultâneo explícito20 mobile/24 PC; não promete100 agentes ativos no celular.
