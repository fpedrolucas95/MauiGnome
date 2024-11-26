# MauiGnome

![MauiGnome](https://github.com/fpedrolucas95/MauiGnome/blob/master/images/mauignome.png)

**MauiGnome** é uma biblioteca MDI (Multiple Document Interface) desenvolvida para **.NET MAUI** e inspirada na interface GNOME. Ela permite gerenciar várias janelas em um único aplicativo, oferecendo funcionalidades como redimensionamento, movimentação e organização multitarefa. 
Sendo totalmente multiplataforma, **MauiGnome** funciona em desktops e dispositivos móveis, com testes realizados no Android e outras plataformas suportadas pelo .NET MAUI.

## 🚀 Funcionalidade

- Gerencie várias janelas dentro de um único container.
- Suporte a janelas redimensionáveis e móveis.
- Personalize facilmente o conteúdo e estilo das janelas.

## 🛠️ Como Implementar

1. **Adicione os arquivos ao seu projeto MAUI**:
   * Crie uma pasta `Controls` no seu projeto
   * Adicione os arquivos `MDIContainer.cs` e `MDIWindow.cs` à pasta

2. **Configure o `MDIContainer` no layout principal**:
   No arquivo XAML, inclua o container que gerenciará as janelas:

```xaml
<controls:MDIContainer x:Name="MDIContainer" 
    Windows="{Binding Windows}"
    ActiveWindow="{Binding ActiveWindow, Mode=TwoWay}" 
    HorizontalOptions="Fill" 
    VerticalOptions="Fill"
    Background="Transparent"/>
```

3. **Adicione janelas dinamicamente**:
   No código, crie instâncias de MDIWindow e adicione-as ao container. A propriedade `WindowContent` aceita qualquer ContentView, permitindo que você carregue seus próprios arquivos XAML como conteúdo da janela:

```csharp
var window = new MDIWindow
{
    Title = "Nova Janela",
    Icon = "icon.png",
    WindowContent = new MinhaView() // Seu próprio ContentView/Page XAML
};
mdiContainer.Windows.Add(window);
```

4. **Defina a janela ativa (opcional)**:
   Para ativar uma janela específica:

```csharp
mdiContainer.SetActiveWindow(window);
```

## 🧪 Exemplo de Aplicação

Este repositório inclui uma aplicação de exemplo que demonstra o uso da biblioteca:
- Uma calculadora funcional que pode ser aberta em múltiplas janelas
- O icônico papel de parede do Windows XP como fundo
- Perfeito para entender a implementação e funcionalidades do MauiGnome

## 📜 Licença

Este projeto está sob a licença AGPL-3.0. Esta licença garante que você pode usar, modificar e distribuir o código, desde que mantenha o código fonte aberto e disponível. Para mais detalhes, consulte o arquivo [LICENSE.txt](https://github.com/fpedrolucas95/MauiGnome/blob/main/LICENSE.txt).
