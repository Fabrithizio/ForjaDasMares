# Equipagem — 0.15.1

## Alterações
- Pool identifica efeitos ativos por dono. Projéteis e áreas de outros atores não bloqueiam a equipagem do jogador fora de perigo.
- Autoridade centraliza as condições e seus motivos: conexão, carregamento, ataque em curso, morte, encontro/perigo e efeitos próprios ainda ativos.
- A interface do host usa a mesma permissão da autoridade. O painel de comparação mostra o motivo de bloqueio ou a resposta da operação.
- Host devolve recusas válidas ao solicitante pelo canal de perfil existente, sem alterar inventário ou inventar sucesso. Respostas são separadas por jogador e não ficam substituindo mensagens futuras de recompensas.
- Mantidas restrições de classe, nível, slot, arma de duas mãos e item equipado em outro herói. Preservados saves v2, pacote Android e protocolo10.

## Arquivos alterados
- Assets/ForjaDasMares/Scripts/Gameplay/CombatEffectPool.cs
- Assets/ForjaDasMares/Networking/CoopProgression.cs
- Assets/ForjaDasMares/Networking/CoopProfileChannel.cs
- Assets/ForjaDasMares/Networking/CoopInventoryHud.cs
- Assets/ForjaDasMares/Networking/Tests/CoopProgressionTests.cs
- Assets/ForjaDasMares/Networking/Editor/CampaignInstaller.cs
- ProjectSettings/ProjectSettings.asset (versão da build)
- Docs/PROGRESSO.md e este relatório; evidência de testes anexa.

## Validação executada
Unity 6000.3.26f1: suíte PlayMode CoopProgressionTests final **8/8 aprovados**, zero falhas. Inclui fluxo real de encontros/chefe/conclusão, baú/morte/persistência, equipagem e rejeição por incompatibilidade, permissão com motivo e isolamento de dono de efeitos. O novo teste de Arcanista relê o perfil após a transação, pois o serviço substitui o objeto de dados ao salvar.

A versão 0.15.0/código31 foi instalada primeiro no moto g15 por `adb install -r`: Success; versão conferida no pacote com.forjadasmares.campaign. Sem desinstalação ou limpeza de dados.

## Builds e instalação final
Windows e Android 0.15.1 gerados com `CAMPAIGN_BUILD ... Succeeded errors=0` e processo Unity encerrado com código 0 em ambos. Caminhos estáveis: `Builds/Atual/Windows/ForjaDasMaresCampanha.exe` e `Builds/Atual/Android/ForjaDasMaresCampanha.apk`. Android código32, ARM64, mesmo pacote. Instalação final via USB `install -r`: Success; `dumpsys package` confirmou 0.15.1/código32 no moto g15. Sem desinstalar ou apagar saves. Nenhuma configuração manual nova de serviços.

## Teste manual
1. Abrir Personagem no menu inicial, selecionar Prisma/Arcanista e filtrar a mochila pela classe.
2. Selecionar um cajado compatível e EQUIPAR; conferir slot e padrão de projétil. Tentar arma de guerreiro: a restrição de classe permanece.
3. Em sala, afastar-se do combate e equipar. No host, durante ataque/perigo/efeito próprio, o painel deve informar a condição que impede a troca.
4. Outro jogador atacar longe não deve impedir sua troca. Em cliente remoto, uma recusa de autoridade deve retornar no painel em vez de ficar silenciosa.

## Limitações e próximos blocos
- Testes automatizados foram no Editor PC. Esta mudança ainda exige conferência visual em celular e sessão real de dois jogadores; instalação não significa teste de jogabilidade.
- Réplica não conhece cada efeito próprio ativo no host: pode mostrar EQUIPAR disponível antes do host permitir. Host revalida e responde o motivo, sem aplicar a troca. Não foi criada nova replicação de permissões nesta correção.
- Mira do Arcanista em movimento ainda não corrigida; próxima prioridade. Identificação geral de itens, organização de mochila e fluxo de menus também continuam no plano da análise.
- Arte, balanceamento e desempenho não foram declarados finalizados.
