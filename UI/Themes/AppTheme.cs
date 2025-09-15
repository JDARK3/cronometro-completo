namespace CronometroRegressivo.UI.Themes
{
    /// <summary>
    /// Classe estática que define o tema visual da aplicação
    /// 
    /// Padrões e conceitos utilizados:
    /// - Singleton Pattern: Classe estática acessível globalmente
    /// - Facade Pattern: Fornece interface simplificada para recursos visuais
    /// - Constants Pattern: Valores imutáveis para consistência visual
    /// - Separation of Concerns: Centraliza todas as definições de tema
    /// </summary>
    public static class AppTheme
    {


          
        public static Color TitleBarColor = Color.FromArgb(45, 45, 45);
        public static Color ButtonHoverColor = Color.FromArgb(60, 60, 60);
        public static Color CloseButtonHoverColor = Color.FromArgb(232, 17, 35);
        /// <summary>
        /// Cor de fundo principal da aplicação (RGB: 7, 3, 17)
        /// - Cor escura para melhor contraste
        /// - Valor fixo para consistência visual em toda a aplicação
        /// </summary>
        public static Color BackgroundColor => Color.FromArgb(14, 13, 20);// color: rgb(14, 13, 20)
    

        /// <summary>
        /// Cor padrão para texto na aplicação
        /// - Branco para melhor legibilidade sobre o fundo escuro
        /// - Usado em labels, textos e outros elementos
        /// </summary>
        public static Color TextColor => Color.White;

        /// <summary>
        /// Fonte para exibição do temporizador principal
        /// - "Fira Code": Fonte monoespaçada para melhor alinhamento de números
        /// - 36pt: Tamanho grande para boa visibilidade
        /// - Bold: Negrito para melhor contraste
        /// </summary>
        public static Font TimerFont => new Font("Rampart_One", 36, FontStyle.Bold);

        /// <summary>
        /// Fonte para títulos e cabeçalhos
        /// - "Segoe UI": Fonte padrão do Windows para melhor legibilidade
        /// - 12pt: Tamanho padrão para textos de interface
        /// - Bold: Destaque para elementos de título
        /// </summary>
        public static Font TitleFont => new Font("Segoe UI", 12, FontStyle.Bold);

        /// <summary>
        /// Cor do ícone de play/start
        /// - Verde (RGB: 0, 200, 0) para indicar ação positiva/iniciar
        /// - Padronizado em toda a aplicação
        /// </summary>
        public static Color PlayIconColor => Color.FromArgb(0, 200, 0);

        /// <summary>
        /// Cor do ícone de pause
        /// - Amarelo (RGB: 200, 200, 0) para indicar ação de atenção/pausa
        /// - Padronizado em toda a aplicação
        /// </summary>
        public static Color PauseIconColor => Color.FromArgb(200, 200, 0);

        /// <summary>
        /// Cor do ícone de reset/reiniciar
        /// - Vermelho (RGB: 200, 0, 0) para indicar ação crítica/reset
        /// - Padronizado em toda a aplicação
        /// </summary>
        public static Color ResetIconColor => Color.FromArgb(200, 0, 0);

        /// <summary>
        /// Cor do ícone de menu
        /// - Cinza (RGB: 180, 180, 180) para ação neutra/navegação
        /// - Padronizado em toda a aplicação
        /// </summary>
        public static Color MenuIconColor => Color.FromArgb(180, 180, 180);

        /// <summary>
        /// Método para carregar ícones vetoriais (implementação opcional)
        /// 
        /// Observação:
        /// - Atualmente retorna null, pois a implementação real está em IconService
        /// - Mantido aqui para possível expansão futura do tema
        /// - Pode ser usado como facade para o IconService
        /// </summary>
        /// <param name="iconName">Nome do ícone a ser carregado</param>
        /// <param name="color">Cor do ícone</param>
        /// <returns>Bitmap com o ícone renderizado</returns>
        public static Bitmap? LoadVectorIcon(string iconName, Color color)
        {
            // Implementação deveria delegar para IconService:
            // return IconService.LoadVectorIcon(iconName, color);
            return null;
        }
    }
}