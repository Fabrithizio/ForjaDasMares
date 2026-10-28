# Assets de cenário da Costa
Origem: sete ZIPs Meshy fornecidos pelo usuário. Os arquivos originais externos permanecem intocados.

Reprodução: Python com Pillow e NumPy: `prepare_sources.py <pasta-dos-zips>`. Depois Blender 4.5 em background executando `prepare_models.py`. Por fim Unity: `ForjaDasMares.Networking.Editor.RegionalWorldInstaller.Apply`.

Assets usados pelo jogo, prefabs, materiais, texturas 1K e três LODs são versionados em Assets/ForjaDasMares/Art/World/Coast. LOD0 conserva a contagem original de triângulos com pivô/escala normalizados; LOD1/2 usam Decimate a 50%/22%. Valores reais em lod_report.json. Texturas metallic e roughness convertidas para metallic RGB/smoothness alpha; normal marcada como NormalMap.

Sources/ e jobs.json são staging local ignorado pelo Git. Os ZIPs externos e as texturas originais 2K precisam de backup próprio; o repositório inclui as versões 1K usadas no jogo, não um arquivo integral desses ZIPs. Não são assets de terceiros baixados por esta tarefa, nem modelos/animações novos criados do zero. As imagens 2D de inimigos não foram convertidas em inimigos 3D nesta entrega.
