# MauiGnome

**MauiGnome** é uma biblioteca MDI (Multiple Document Interface) desenvolvida para **.NET MAUI** e inspirada na interface GNOME. Ela permite gerenciar várias janelas em um único aplicativo, oferecendo funcionalidades como redimensionamento, movimentação e organização multitarefa. 

Sendo totalmente multiplataforma, **MauiGnome** funciona em desktops e dispositivos móveis.

## 🚀 Funcionalidades
- Gerenciamento de múltiplas janelas em um container
- Janelas redimensionáveis e móveis
- Interface personalizável
- Suporte a temas claro/escuro

## 🛠️ Como Implementar

1. **Adicione os arquivos base**:
   * Clone os arquivos `MDIContainer.cs` e `MDIWindow.cs` para uma pasta `Controls` no seu projeto
   * Adicione a referência no XAML:
```xaml
xmlns:controls="clr-namespace:YourNamespace.Controls"
```

2. **Adicione o Container MDI**:
```xaml
<controls:MDIContainer 
    Windows="{Binding Windows}"
    ActiveWindow="{Binding ActiveWindow, Mode=TwoWay}" 
    HorizontalOptions="Fill" 
    VerticalOptions="Fill"/>
```

3. **Crie suas janelas**:
```csharp
// Crie uma nova janela
var window = new MDIWindow
{
    Title = "Minha Janela",
    WindowContent = new MinhaView(),
    WindowWidth = 400,
    WindowHeight = 300
};

// Adicione ao container
Windows.Add(window);
```

4. **Gerencie suas janelas**:
```csharp
// Ative uma janela
window.Activate();

// Feche uma janela
window.Close();
```

## 📦 Recursos Incluídos

1. **Gerenciamento de Janelas**:
   - Movimentação
   - Redimensionamento

2. **Personalização**:
   - Temas claro/escuro
   - Ícones personalizados
   - Estilos customizáveis

3. **Eventos**:
   - Ativação/Desativação
   - Abertura/Fechamento
   - Alteração de estado

## 🧪 Exemplo Prático

Este repositório inclui algumas aplicações de exemplo demonstrando:
- Calculadora funcional com múltiplas instâncias
- Aplicativo de desenho com seleção de cores
- Gráfico de candles
- Interface inspirada no GNOME
- Implementação completa dos recursos

## 📜 Licença

Este projeto está sob a licença AGPL-3.0. Consulte o arquivo [LICENSE.txt](https://github.com/fpedrolucas95/MauiGnome/blob/master/LICENSE.txt) para mais detalhes.
