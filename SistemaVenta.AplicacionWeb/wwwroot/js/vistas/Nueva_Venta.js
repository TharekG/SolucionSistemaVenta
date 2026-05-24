let ValorImpuesto = 0;
let ProductosParaVenta = [];

$(document).ready(function () {

    // Cargar tipos de documento
    fetch("/Venta/ListaTipoDocumentoVenta")
        .then(r => r.ok ? r.json() : Promise.reject(r))
        .then(responseJson => {
            if (responseJson.length > 0) {
                responseJson.forEach(item => {
                    // Slide 26: en vez de Boleta → Ticket o Factura
                    let desc = item.descripcion;
                    if (desc.toLowerCase() === "boleta") desc = "Ticket";
                    $("#cboTipoDocumentoVenta").append(
                        $("<option>").val(item.idTipoDocumentoVenta).text(desc)
                    );
                });
            }
        });

    // Cargar datos del negocio
    fetch("/Negocio/Obtener")
        .then(r => r.ok ? r.json() : Promise.reject(r))
        .then(responseJson => {
            if (responseJson.estado) {
                const d = responseJson.objeto;
                $("#inputGroupSubTotal").text(`Sub Total - ${d.simboloMoneda}`);
                $("#inputGroupIGV").text(`IVA - ${d.simboloMoneda}`);
                $("#inputGroupTotal").text(`Total - ${d.simboloMoneda}`);
                ValorImpuesto = parseFloat(d.porcentajeImpuesto);
            }
        });

    // Select2 para buscar productos
    $("#cboBuscarProducto").select2({
        ajax: {
            url: "/Venta/ObtenerProductos",
            dataType: "json",
            delay: 250,
            data: function (params) { return { busqueda: params.term }; },
            processResults: function (data) {
                return {
                    results: data.map(item => ({
                        id: item.idProducto,
                        text: item.descripcion,
                        marca: item.descripcionMarca || item.marca || "",
                        categoria: item.descripcionCategoria || item.nombreCategoria || "",
                        urlImagen: item.urlImagen,
                        precioventa: parseFloat(item.precioventa || item.precio || 0),
                        descuento: parseFloat(item.descuento || 0),
                        impuestoproducto: parseFloat(item.impuestoproducto || 0)
                    }))
                };
            }
        },
        language: "es",
        placeholder: "Buscar Producto...",
        minimumInputLength: 1,
        templateResult: formatoResultados
    });

    // ── Buscar Cliente por RFC ──────────────────────────────────────
    $("#btnBuscarCliente").click(function () {
        const rfc = $("#txtRfcCliente").val().trim().toUpperCase();
        if (!rfc) {
            toastr.warning("", "Ingrese un RFC para buscar");
            return;
        }
        fetch(`/Clientes/ObtenerPorRfc?rfc=${rfc}`)
            .then(r => r.ok ? r.json() : Promise.reject(r))
            .then(responseJson => {
                if (responseJson.estado && responseJson.objeto) {
                    const cliente = responseJson.objeto;
                    $("#txtIdCliente").val(cliente.idCliente);
                    $("#txtNombreCliente").val(cliente.nombreCliente);
                    toastr.success("", `Cliente encontrado: ${cliente.nombreCliente}`);
                } else {
                    // No encontrado → Público en General
                    $("#txtIdCliente").val(0);
                    $("#txtNombreCliente").val("Público en General");
                    toastr.info("", "RFC no encontrado. Se usará Público en General.");
                }
            })
            .catch(() => {
                $("#txtIdCliente").val(0);
                $("#txtNombreCliente").val("Público en General");
            });
    });

    // RFC en mayúsculas automático
    $("#txtRfcCliente").on("input", function () {
        $(this).val($(this).val().toUpperCase());
    });

    // ── Terminar Venta ──────────────────────────────────────────────
    $("#btnTerminarVenta").click(function () {

        if (ProductosParaVenta.length < 1) {
            toastr.warning("", "Debe ingresar productos.");
            return;
        }

        const idCliente = parseInt($("#txtIdCliente").val()) || null;

        const venta = {
            idTipoDocumentoVenta: $("#cboTipoDocumentoVenta").val(),
            idCliente: idCliente,
            documentoCliente: $("#txtRfcCliente").val().substring(0, 13),
            nombreCliente: $("#txtNombreCliente").val() || "Público en General",
            subTotal: $("#txtSubTotal").val(),
            impuestoTotal: $("#txtIGV").val(),
            total: $("#txtTotal").val(),
            DetalleVenta: ProductosParaVenta
        };

        $("#btnTerminarVenta").LoadingOverlay("show");

        fetch("/Venta/RegistrarVenta", {
            method: "POST",
            headers: { "Content-Type": "application/json; charset=utf-8" },
            body: JSON.stringify(venta)
        })
            .then(r => {
                $("#btnTerminarVenta").LoadingOverlay("hide");
                return r.ok ? r.json() : Promise.reject(r);
            })
            .then(responseJson => {
                if (responseJson.estado) {
                    ProductosParaVenta = [];
                    mostrarProducto_Precios();
                    $("#txtIdCliente").val(0);
                    $("#txtRfcCliente").val("");
                    $("#txtNombreCliente").val("");
                    $("#cboTipoDocumentoVenta").val($("#cboTipoDocumentoVenta option:first").val());
                    swal("Registrado!", `Numero Venta: ${responseJson.objeto.numeroVenta}`, "success");
                } else {
                    swal("Lo sentimos!", responseJson.mensaje || "No se pudo registrar la venta", "error");
                }
            });
    });
});

// ── Formato resultados Select2 ────────────────────────────────────────────
function formatoResultados(data) {
    if (data.loading) return data.text;
    return $(`<table width="100%">
        <tr>
            <td style="width:60px">
                <img style="height:60px;width:60px;margin-right:10px" src="${data.urlImagen || '/img/noimage.png'}"/>
            </td>
            <td>
                <p style="font-weight:bolder;margin:2px">${data.marca}</p>
                <p style="margin:2px">${data.text}</p>
            </td>
        </tr>
    </table>`);
}

$(document).on("select2:open", function () {
    document.querySelector(".select2-search__field").focus();
});

// ── Seleccionar Producto ──────────────────────────────────────────────────
$("#cboBuscarProducto").on("select2:select", function (e) {
    const data = e.params.data;

    if (ProductosParaVenta.filter(p => p.idProducto == data.id).length > 0) {
        $("#cboBuscarProducto").val("").trigger("change");
        toastr.warning("", "El producto ya fue agregado");
        return false;
    }

    swal({
        title: data.marca,
        text: data.text,
        imageUrl: data.urlImagen,
        type: "input",
        showCancelButton: true,
        closeOnConfirm: false,
        inputPlaceholder: "Ingrese Cantidad"
    }, function (valor) {
        if (valor === false) return false;
        if (valor === "") { toastr.warning("", "Necesita ingresar la cantidad."); return false; }
        if (isNaN(parseInt(valor))) { toastr.warning("", "Debe ingresar un valor numérico."); return false; }

        const cantidad = parseInt(valor);
        const precioUnitario = data.precioventa;
        const descuentoPct = data.descuento; // ej: 0.15 = 15%
        const impuestoPct = data.impuestoproducto; // ej: 0.16 = 16%

        const subtotalAntesDescuento = cantidad * precioUnitario;
        const descuentoDinero = subtotalAntesDescuento * descuentoPct;
        const subtotalConDescuento = subtotalAntesDescuento - descuentoDinero;
        const impuestoDinero = subtotalConDescuento * impuestoPct;
        const totalProducto = subtotalConDescuento + impuestoDinero;

        const producto = {
            idProducto: data.id,
            marcaProducto: data.marca,
            descripcionProducto: data.text,
            categoriaProducto: data.categoria,
            cantidad: cantidad,
            precio: precioUnitario.toFixed(2),
            preciodeventa: precioUnitario.toFixed(2),
            subtotalantesdescuento: subtotalAntesDescuento.toFixed(2),
            descuentoenporcentaje: descuentoPct.toFixed(6),
            descuentoendinero: descuentoDinero.toFixed(2),
            subtotalcondescuento: subtotalConDescuento.toFixed(2),
            impuestoenporcentaje: impuestoPct.toFixed(6),
            impuestoendinero: impuestoDinero.toFixed(2),
            totalporproducto: totalProducto.toFixed(2),
            total: totalProducto.toFixed(2)
        };

        ProductosParaVenta.push(producto);
        mostrarProducto_Precios();
        $("#cboBuscarProducto").val("").trigger("change");
        swal.close();
    });
});

// ── Mostrar Productos y Totales ───────────────────────────────────────────
function mostrarProducto_Precios() {

    let totalDescuento = 0;
    let totalImpuesto = 0;
    let totalVenta = 0;
    let subtotalGeneral = 0;

    $("#tbProducto tbody").html("");

    ProductosParaVenta.forEach(item => {
        const desc = parseFloat(item.descuentoendinero);
        const imp = parseFloat(item.impuestoendinero);
        const subtotal = parseFloat(item.subtotalcondescuento);
        const total = parseFloat(item.totalporproducto);

        totalDescuento += desc;
        totalImpuesto += imp;
        subtotalGeneral += subtotal;
        totalVenta += total;

        const descPct = (parseFloat(item.descuentoenporcentaje) * 100).toFixed(0);

        $("#tbProducto tbody").append(
            $("<tr>").append(
                $("<td>").append(
                    $("<button>").addClass("btn btn-danger btn-eliminar btn-sm")
                        .append($("<i>").addClass("fas fa-trash-alt"))
                        .data("idProducto", item.idProducto)
                ),
                $("<td>").text(item.descripcionProducto),
                $("<td>").text(item.cantidad),
                $("<td>").text(`$${parseFloat(item.precio).toFixed(2)}`),
                $("<td>").text(`${descPct}% ($${desc.toFixed(2)})`),
                $("<td>").text(`$${total.toFixed(2)}`)
            )
        );
    });

    $("#txtSubTotal").val(subtotalGeneral.toFixed(2));
    $("#txtDescuentoTotal").val(totalDescuento.toFixed(2));
    $("#txtIGV").val(totalImpuesto.toFixed(2));
    $("#txtTotal").val(totalVenta.toFixed(2));
}

// ── Eliminar Producto ─────────────────────────────────────────────────────
$(document).on("click", "button.btn-eliminar", function () {
    const _idproducto = $(this).data("idProducto");
    ProductosParaVenta = ProductosParaVenta.filter(p => p.idProducto != _idproducto);
    mostrarProducto_Precios();
});
