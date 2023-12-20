namespace BlazorInking.Components;

public partial class InkingCanvas
{
    private ElementReference container;
    private Canvas? _context;
    private Context2D? ctx1;
    private double canvasx;
    private double canvasy;
    private double last_mousex;
    private double last_mousey;
    private double mousex;
    private double mousey;
    private bool mousedown = false;
    private string clr = "black";

    private class Position
    {
        public double Left { get; set; }

        public double Top { get; set; }
    }

    private async Task ToggleColorAsync()
    {
        if (ctx1 is null)
        {
            return;
        }

        clr = clr == "black" ? "red" : "black";
        await ctx1.StrokeStyleAsync(clr);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _context is not null)
        {
            ctx1 = await _context.GetContext2DAsync();
            // initialize settings
            await ctx1.GlobalCompositeOperationAsync(CompositeOperation.Source_Over);
            await ctx1.StrokeStyleAsync(clr);
            await ctx1.LineWidthAsync(3);
            await ctx1.LineJoinAsync(LineJoin.Round);
            await ctx1.LineCapAsync(LineCap.Round);
            // this retrieves the top left corner of the canvas container (which is equivalent to the top left corner of the canvas, as we don't have any margins / padding)
            var p = await js.InvokeAsync<Position>("eval", $"let e = document.querySelector('[_bl_{container.Id}=\"\"]'); e = e.getBoundingClientRect(); e = {{ 'Left': e.x, 'Top': e.y }}; e");
            (canvasx, canvasy) = (p.Left, p.Top);
        }
    }

    private void MouseDownCanvas(MouseEventArgs e)
    {
        render_required = false;
        last_mousex = mousex = e.ClientX - canvasx;
        last_mousey = mousey = e.ClientY - canvasy;
        mousedown = true;
    }

    private void MouseUpCanvas(MouseEventArgs e)
    {
        render_required = false;
        mousedown = false;
    }

    private async Task MouseMoveCanvasAsync(MouseEventArgs e)
    {
        render_required = false;
        if (!mousedown)
        {
            return;
        }
        mousex = e.ClientX - canvasx;
        mousey = e.ClientY - canvasy;
        await DrawCanvasAsync(mousex, mousey, last_mousex, last_mousey, clr);
        last_mousex = mousex;
        last_mousey = mousey;
    }

    private async Task DrawCanvasAsync(double prev_x, double prev_y, double x, double y, string clr)
    {
        if (ctx1 is null)
        {
            return;
        }

        await using var ctx2 = ctx1.CreateBatch();
        await ctx2.BeginPathAsync();
        await ctx2.MoveToAsync(prev_x, prev_y);
        await ctx2.LineToAsync(x, y);
        await ctx2.StrokeAsync();

    }

    private bool render_required = true;

    protected override bool ShouldRender()
    {
        if (!render_required)
        {
            render_required = true;
            return false;
        }
        return base.ShouldRender();
    }
}
