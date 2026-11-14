# Nova personagem principal — 0.16.1

Skin Sapphire Mage aplicada ao Arcanista na cena CampaignCoop. Seleção inicial passa aArcanista; atributos, habilidades, inventário, armas e rede preservados. Um único modelo por ator, material URP com texturas originais e mapas de superfície convertidos. Encaixes das armas e efeitos mantidos. Assets Prisma e cenas de referência antigos preservados para retorno.

Animações explicitamente adiadas pelo usuário: controlador com a pose real fornecida pelo FBX, marcado authoredAnimationsReady=false. Não são reproduzidos clipes do outro esqueleto. Os dois testes específicos do conjunto antigo de animações ficam ignorados enquanto esse estado estiver ativo; não foram declarados aprovados para a nova skin.

Instalação Unity concluída com saída0, corpo1,7m,12.413 triângulos/76 ossos. Build Windows Succeeded/errors0/exit0; jogo aberto e visual conferido no inventário com o equipamento existente do perfil; deixado aberto para avaliação do usuário. Não foi executada suíte de animações da nova skin. Sem novo APK ou teste físico Android.

Arquivos principais: Editor/SapphireMageInstaller.cs, Art/Characters/SapphireMage, Scenes/CampaignCoop.unity, HeroModelView.cs, CoopDirector.cs, CampaignInstaller.cs e PrismaTests.cs. Nenhuma nova animação criada.
