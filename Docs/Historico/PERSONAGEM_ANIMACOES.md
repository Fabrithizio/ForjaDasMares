# Atualização 0.17.0

O conjunto inicial de 25 clips da nova personagem foi criado no Blender e integrado; efeitos próprios e refinamentos de espada/machado também foram entregues. A lista abaixo registra a recomendação anterior de aquisição, não um bloqueio atual. Para estado, testes e limites atuais: ANIMACOES_E_EFEITOS_0_17_0.md.

# Personagem principal: escala e próximos recursos — 0.16.2

## Implementado

Sapphire Mage continua sendo o visual principal do Arcanista. HeroModelView possui limites de tamanho de equipamento configuráveis no Inspector, habilitados somente nessa skin. Comprimento máximo: cajado 1,45 m; grimório 0,38 m; escudo 0,60 m; foco mágico 0,30 m; demais armas 0,95 m. Itens menores preservam seu tamanho. A redução é uniforme, inclusive no deslocamento do encaixe. O fator é aplicado após compensar a escala do esqueleto, tanto no equipamento local quanto nas réplicas online, pelo mesmo EquipmentVisualPlacement/EquipmentMeshView. Dano, alcance, salvamento, ícones e outros heróis não mudam.

O instalador verifica as dimensões efetivas de 16 equipamentos importados, restaura o cajado inicial e grava o prefab/cena. Reexecutar pelo menu Forja das Mares/Characters/Install Sapphire Mage visual preview. Os limites podem ser ajustados no componente do prefab sem modificar os dados de combate.

## Animações: o que obter primeiro

Recomendação principal: abrir a personagem ORIGINAL no Meshy, acessar Animate e escolher movimentos no mesmo esqueleto. Exportar FBX com animação, de preferência um arquivo por movimento, mantendo nomes claros. Não gerar outro corpo e não alterar a escala entre exportações. Manter o modelo com skin na exportação se essa for a opção oferecida; podemos separar o clip na importação. Não basta enviar vídeo ou GIF.

Primeiro pacote de 10 movimentos:

1. Idle/Breathing Idle: respiração discreta, postura de maga, mãos livres.
2. Walk: caminhada no lugar, sem deslocamento da raiz.
3. Run: corrida no lugar.
4. Strafe Left: deslocamento lateral no lugar.
5. Strafe Right: deslocamento lateral no lugar.
6. Magic Cast: disparo curto para frente.
7. Heavy Magic Cast: preparação e disparo forte.
8. Dodge: passo evasivo rápido.
9. Hit Reaction: reação curta ao dano.
10. Death: queda que termina no chão.

Depois: andar para trás, segunda/terceira conjuração, canalização em loop, ritual do super, levantar, interação e vitória. Isso resulta em cerca de 18–20 clips, com reaproveitamento entre poderes. Priorizar movimento e ataque antes de efeitos grandes. O salto giratório fornecido não substitui esse conjunto e não foi ligado arbitrariamente a uma habilidade.

Alternativa gratuita: Mixamo com Adobe ID. Enviar o FBX, testar um idle/corrida e baixar FBX para Unity, preferencialmente In Place quando disponível. Antes de baixar dezenas de arquivos, enviar primeiro 2–3 para conferir retarget e pés. Não refazer o rig original sem guardar a versão atual. Mixamo permite uso comercial dos personagens/animações dentro do jogo; não republicar arquivos brutos de bibliotecas externas no repositório público. Não foram baixados assets de terceiros nesta alteração.

## Efeitos

Já existe PrismaCastingView com glifos nas mãos, carregamento e pulso de habilidade/super, ligado aos eventos reais de apresentação. Ele é preservado. Não é preciso comprar VFX agora: podemos criar os efeitos na Unity quando os tempos das animações estiverem definidos.

Direção: azul/ciano e dourado; preparo curto, disparo legível, impacto rápido. Ataque comum pequeno; carregado com crescente intensidade; super mais marcante. Evitar cobrir inimigos/perigos, luzes dinâmicas em cada projétil e partículas excessivas com quatro jogadores. Efeito visual não altera área real de dano. Para o primeiro acabamento: preparo nas mãos, projétil/rastro, impacto normal, crítico, carregado, habilidade, super e esquiva. Reutilizar materiais e pooling existentes.

## Limitações e validação

O FBX atual contém apenas pose de repouso. Ainda não há caminhada/conjurações finalizadas para a nova skin; ela continua rígida em movimento. Não foram declarados clips criados ou retargetados. Não há novo APK nem teste físico Android nesta entrega. Testes de escala não substituem avaliação de pegada e clipping durante animações, que serão revisados ao receber os clips.

Fontes verificadas em 26/10/2026:
- https://docs.meshy.ai/en/webapp/guides/animate
- https://helpx.adobe.com/in/creative-cloud/faq/mixamo-faq.html

Validação desta entrega: instalador Unity exit 0, 16 equipamentos; build Windows Succeeded/errors=0; cajado conferido visualmente no inventário em 1280×720. Demais categorias verificadas por dimensões, sem teste visual individual ou teste de rede nesta rodada.
