# Forja das Mares — PvE 0.3.0

Abra a Unity no projeto Área de Trabalho/Jogos/ForjaDasMares. F6 abre ArenaTest; pressione Play. O executável fica em Builds/Windows/ForjaDasMares.exe.

WASD/setas: andar. Mouse: apontar, esquerdo atacar, direito ou E habilidade. Espaço: esquiva. 1–4/Tab: heróis. R: reiniciar. Use PROXIMO ENCONTRO para escolher ondas, cada arquétipo ou chefe; INICIAR ENCONTRO restaura o herói e inicia. Trocar de herói cancela o encontro, então inicie novamente. H/V são botões de teste de dano/vitória e não completam encontros.

Evite as áreas coloridas: o golpe ocorre depois do aviso. O guardião bloqueia pela frente; flanqueie. O arqueiro recua. O instável explode após o aviso; a explosão também o elimina. O invocador cria até dois fragmentos vivos, com orçamento de seis invocações. O chefe usa arco, três marcas no chão e investida; muda de fase a 50% da vida. A recompensa são selos voláteis de treino.

Android mantém joystick, ataque, habilidade, esquiva, herói, reset e botões de encontros. Nesta etapa o aparelho está com o usuário; testes físicos ficam pendentes.

Atributos em Assets/ForjaDasMares/Data/Enemies. Ondas em Data/Encounters. Prefabs em Prefabs/Enemies; NavMeshData em Navigation. Limites de inimigos, tempos de aviso, dano e invocação podem ser ajustados nos ScriptableObjects. Rebake de navegação é necessário ao modificar obstáculos; dois NavMeshSurface, normal e chefe. Os modelos são geometria original provisória e as poses são procedurais, sem clips de animação final.
