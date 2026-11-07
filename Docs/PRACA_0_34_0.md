# Refúgio — polimento 0.34.0

A montagem futurista remanescente era AncientMemorySite, independente da antiga entrada da estação. Os renderizadores e colisores da montagem foram desativados. O novo santuário reaproveita o altar do kit local; o ponto de interação continua ativo, acessível e revela uma fala sobre runas, sem mudar a missão.

42 árvores foram ajustadas pela base do LOD detalhado contra o piso ou a malha real do terreno. O colisor usado para medir a superfície só existe durante a migração no Editor. A pira foi afastada da fonte de descanso, que recebeu duas floreiras sem sombras. Bordas do pavimento principal suavizadas em malha estática, com UVs contínuos e vértices compartilhados. Os caminhos do Jardim Norte ainda têm trechos retos.

Iluminação usa o sol existente, ambiente mais frio e um perfil URP leve de cor/contraste/exposição; sem luzes novas, bloom ou ambient occlusion. Nenhum asset externo importado neste bloco. Não houve benchmark antes/depois.

Arquivos: TownPlazaPolish.cs, TownTests.cs, CampaignInstaller.cs, RefugioUltimoSino.unity, Navigation.asset, CityPathsRounded.asset, TownColorGrade.asset e versão do projeto. Testes: 29/29 PlayMode. Build Windows Succeeded/errors=0. Captura da praça de chegada no executável em Validation/0.34.0/praca.png. Sem APK ou teste físico Android.

Para testar: abrir o RPG, caminhar até a fonte de descanso, inspecionar o santuário no lugar da máquina e comparar os perfis gráficos no menu. F8 exibe FPS local. Pendentes: acabamento dos outros caminhos, arquitetura/vegetação e efeitos novos, medição comparativa no PC e Android.
