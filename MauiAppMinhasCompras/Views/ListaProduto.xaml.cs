using MauiAppMinhasCompras.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace MauiAppMinhasCompras.Views;

public partial class ListaProduto : ContentPage
{
    // Esta � a lista que a ListView exibe.
    private ObservableCollection<Produto> lista = new ObservableCollection<Produto>();

    // Esta � a c�pia completa de todos os produtos do banco de dados.
    private List<Produto> listaCompleta = new List<Produto>();

    public ListaProduto()
    {
        InitializeComponent();
        lst_produtos.ItemsSource = lista;
    }

    protected override async void OnAppearing()
    {
        await LoadProducts();
    }

    // Este m�todo carrega os produtos do banco de dados e os filtros.
    private async Task LoadProducts()
    {
        try
        {
            // Pega todos os produtos e armazena na lista completa.
            listaCompleta = await App.Db.GetAll();

            // Atualiza a lista da tela com os produtos carregados e filtrados.
            await UpdateListView();

            // Popula o Picker com as categorias.
            LoadCategories();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    // M�todo principal para filtrar a lista.
    private async Task UpdateListView()
    {
        lista.Clear(); // Limpa a lista exibida na tela.

        string filtroBusca = txt_search.Text?.ToLower();
        string filtroCategoria = picker_categoria.SelectedItem?.ToString();

        // Usa LINQ para aplicar os filtros.
        var produtosFiltrados = listaCompleta.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(filtroBusca))
        {
            produtosFiltrados = produtosFiltrados.Where(p => p.Descricao.ToLower().Contains(filtroBusca));
        }

        if (!string.IsNullOrWhiteSpace(filtroCategoria) && filtroCategoria != "Todas")
        {
            produtosFiltrados = produtosFiltrados.Where(p => p.Categoria == filtroCategoria);
        }

        foreach (var p in produtosFiltrados.OrderBy(p => p.Descricao))
        {
            lista.Add(p); // Adiciona os itens filtrados na lista exibida.
        }
    }

    // M�todo para popular o Picker de Categorias.
    private void LoadCategories()
    {
        var categorias = listaCompleta.Select(p => p.Categoria).Distinct().ToList();
        categorias.Insert(0, "Todas"); // Adiciona a op��o para mostrar tudo.
        picker_categoria.ItemsSource = categorias;
    }

    // L�gica para o bot�o "Adicionar".
    private void ToolbarItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            Navigation.PushAsync(new Views.NovoProduto());
        }
        catch (Exception ex)
        {
            DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    // L�gica para o bot�o "Somar" (renomeado para "Relat�rio" na minha sugest�o anterior).
    private async void ToolbarItem_Clicked_1(object sender, EventArgs e)
    {
        // L�gica do relat�rio para somar por categoria.
        var totalPorCategoria = listaCompleta
            .GroupBy(p => p.Categoria)
            .Select(g => new { Categoria = g.Key, Total = g.Sum(p => p.Total) })
            .ToList();

        string msg = "Relat�rio de Gastos por Categoria:\n\n";
        foreach (var item in totalPorCategoria)
        {
            msg += $"{item.Categoria}: {item.Total:C}\n";
        }

        await DisplayAlert("Relat�rio de Compras", msg, "OK");
    }

    // L�gica para a busca no SearchBar.
    private async void txt_search_TextChanged_1(object sender, TextChangedEventArgs e)
    {
        await UpdateListView();
    }

    // L�gica para o filtro de categoria.
    private async void picker_categoria_SelectedIndexChanged(object sender, EventArgs e)
    {
        await UpdateListView();
    }

    // L�gica para o menu de remo��o.
    private async void MenuItem_Clicked(object sender, EventArgs e)
    {
        try
        {
            MenuItem selecionado = sender as MenuItem;
            Produto p = selecionado.BindingContext as Produto;

            bool confirm = await DisplayAlert("Tem Certeza?", $"Remover {p.Descricao}?", "Sim", "N�o");

            if (confirm)
            {
                await App.Db.Delete(p.Id);
                listaCompleta.Remove(p); // Remove da lista completa.
                await UpdateListView(); // Atualiza a lista na tela.
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    // L�gica para quando um item � selecionado na lista.
    private void lst_produtos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        try
        {
            Produto p = e.SelectedItem as Produto;

            Navigation.PushAsync(new Views.EditarProduto
            {
                BindingContext = p,
            });
        }
        catch (Exception ex)
        {
            DisplayAlert("Ops", ex.Message, "OK");
        }
    }

    // L�gica para "pull-to-refresh".
    private async void lst_produtos_Refreshing(object sender, EventArgs e)
    {
        lst_produtos.IsRefreshing = true;
        await LoadProducts();
        lst_produtos.IsRefreshing = false;
    }

    private void ToolbarItem_Clicked_2(object sender, EventArgs e)
    {
        double soma = listaCompleta.Sum(i => i.Total);
        string msg = $"O total � {soma:C}";
        DisplayAlert("Total dos Produtos", msg, "OK");
    }
}