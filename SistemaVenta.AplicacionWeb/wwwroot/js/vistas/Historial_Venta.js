const VISTA_BUSQUEDA = {
    busquedaFecha: () => {
        $("#txtFechaInicio").val("")
        $("#txtFechaFin").val("")
        $("#txtNumeroVenta").val("")
        $(".busqueda-fecha").show()
        $(".busqueda-venta").hide()
    },
    busquedaVenta: () => {
        $("#txtFechaInicio").val("")
        $("#txtFechaFin").val("")
        $("#txtNumeroVenta").val("")
        $(".busqueda-fecha").hide()
        $(".busqueda-venta").show()
    }
}

$(document).ready(function () {

    VISTA_BUSQUEDA["busquedaFecha"]()

    $.datepicker.setDefaults($.datepicker.regional["es"])
    $("#txtFechaInicio").datepicker({ dateFormat: "dd/mm/yy" })
    $("#txtFechaFin").datepicker({ dateFormat: "dd/mm/yy" })

    cargarUsoCFDI();
    cargarRegimenFiscal();
    cargarFormaPago();
    cargarMetodoPago();
    cargarTipoComprobante();

    // ── Validación PPD → forzar FormaPago 99 ─────────────────────────────
    $("#cboMetodoPago").change(function () {
        const esPPD = $(this).find("option:selected").text().indexOf("PPD") !== -1;
        if (esPPD) {
            let encontrado = false;
            $("#cboFormaPago option").each(function () {
                if ($(this).text().indexOf("99") !== -1) {
                    $("#cboFormaPago").val($(this).val());
                    encontrado = true;
                    return false;
                }
            });
            $("#cboFormaPago").prop("disabled", true);
            if (encontrado)
                toastr.info("Forma de pago establecida en 99 - Por definir (requerido por SAT para PPD)", "");
        } else {
            $("#cboFormaPago").prop("disabled", false);
        }
    });

    // ── Enviar Factura ────────────────────────────────────────────────────
    $("#btnEnviarFactura").click(function () {
        const idVenta = parseInt($("#txtIdVentaFactura").val());
        const idUsoCFDI = parseInt($("#cboUsoCFDI").val()) || null;
        const idRegimenFiscal = parseInt($("#cboRegimenFiscalFactura").val()) || null;
        const idFormaPago = parseInt($("#cboFormaPago").val()) || null;
        const idMetodoPago = parseInt($("#cboMetodoPago").val()) || null;
        const idTipoDeComprobante = parseInt($("#cboTipoComprobante").val()) || null;
        const codigoPostal = $("#txtCPFactura").val().trim();

        if (!idUsoCFDI || !idRegimenFiscal || !idFormaPago || !idMetodoPago || !idTipoDeComprobante || !codigoPostal) {
            toastr.warning("", "Debe completar todos los campos de facturación");
            return;
        }

        const modelo = { idVenta, idUsoCFDI, idRegimenFiscal, idFormaPago, idMetodoPago, idTipoDeComprobante, codigoPostal };

        $("#modalFactura").find("div.modal-content").LoadingOverlay("show");

        fetch("/Venta/SolicitarFactura", {
            method: "POST",
            headers: { "Content-Type": "application/json; charset=utf-8" },
            body: JSON.stringify(modelo)
        })
            .then(response => {
                $("#modalFactura").find("div.modal-content").LoadingOverlay("hide");
                return response.ok ? response.json() : Promise.reject(response);
            })
            .then(responseJson => {
                if (responseJson.estado) {
                    $("#modalFactura").modal("hide");
                    swal("¡Listo!", "Factura solicitada correctamente.", "success");
                    $(`button.btn-factura[data-idventa="${idVenta}"]`)
                        .prop("disabled", true)
                        .removeClass("btn-success")
                        .addClass("btn-secondary")
                        .html('<i class="fas fa-check"></i> Facturado');
                } else {
                    swal("Lo sentimos", responseJson.mensaje, "error");
                }
            })
            .catch(() => swal("Error", "No se pudo conectar con el servidor.", "error"));
    });
});

// ── Cambio de tipo búsqueda ───────────────────────────────────────────────
$("#cboBuscarPor").change(function () {
    if ($(this).val() == "fecha") {
        VISTA_BUSQUEDA["busquedaFecha"]()
    } else {
        VISTA_BUSQUEDA["busquedaVenta"]()
    }
})

// ── Buscar ────────────────────────────────────────────────────────────────
$("#btnBuscar").click(function () {
    if ($("#cboBuscarPor").val() == "fecha") {
        if ($("#txtFechaInicio").val().trim() == "" || $("#txtFechaFin").val().trim() == "") {
            toastr.warning("", "Debe Ingresar fecha inicio y fin")
            return;
        }
    } else {
        if ($("#txtNumeroVenta").val().trim() == "") {
            toastr.warning("", "Debe Ingresar el numero de venta")
            return;
        }
    }

    let numeroVenta = $("#txtNumeroVenta").val()
    let fechaInicio = $("#txtFechaInicio").val()
    let fechaFin = $("#txtFechaFin").val()

    $(".card-body").find("div.row").LoadingOverlay("show");

    fetch(`/Venta/Historial?numeroVenta=${numeroVenta}&fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`)
        .then(response => {
            $(".card-body").find("div.row").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            $("#tbventa tbody").html("");

            if (responseJson.length > 0) {
                responseJson.forEach((venta) => {
                    const yaFacturado = venta.uuid && venta.uuid.trim() !== "";

                    const btnDetalle = $("<button>")
                        .addClass("btn btn-info btn-sm mr-1")
                        .attr("title", "Ver Detalles")
                        .append($("<i>").addClass("fas fa-list-alt"))
                        .append(" Ver Detalles")
                        .data("venta", venta);

                    const btnFactura = $("<button>")
                        .addClass("btn btn-sm btn-factura")
                        .addClass(yaFacturado ? "btn-secondary" : "btn-success")
                        .attr("title", yaFacturado ? "Ya facturado" : "Solicitar Factura")
                        .prop("disabled", yaFacturado)
                        .attr("data-idventa", venta.idVenta)
                        .data("venta", venta)
                        .append($("<i>").addClass(yaFacturado ? "fas fa-check" : "fas fa-file-invoice-dollar"))
                        .append(yaFacturado ? " Facturado" : " Solicitar Factura");

                    $("#tbventa tbody").append(
                        $("<tr>").append(
                            $("<td>").text(venta.fechaRegistro),
                            $("<td>").text(venta.nombreCliente),
                            $("<td>").text(venta.numeroVenta),
                            $("<td>").text(venta.total),
                            $("<td>").append(btnDetalle).append(btnFactura)
                        )
                    )
                })
            }
        })
})

// ── Ver Detalle ───────────────────────────────────────────────────────────
$("#tbventa tbody").on("click", ".btn-info", function () {
    let d = $(this).data("venta")

    $("#txtFechaRegistro").val(d.fechaRegistro)
    $("#txtNumVenta").val(d.numeroVenta)
    $("#txtUsuarioRegistro").val(d.usuario)
    $("#txtTipoDocumento").val(d.tipoDocumentoVenta)
    $("#txtDocumentoCliente").val(d.documentoCliente)
    $("#txtNombreCliente").val(d.nombreCliente)
    $("#txtSubTotal").val(d.subTotal)
    $("#txtIGV").val(d.impuestoTotal)
    $("#txtTotal").val(d.total)

    $("#tbProductos tbody").html("");
    d.detalleVenta.forEach((item) => {
        $("#tbProductos tbody").append(
            $("<tr>").append(
                $("<td>").text(item.descripcionProducto),
                $("<td>").text(item.cantidad),
                $("<td>").text(item.precio),
                $("<td>").text(item.total),
            )
        )
    })

    $("#linkImprimir").attr("href", `/Venta/MostrarPDFVenta?numeroVenta=${d.numeroVenta}`)
    $("#modalData").modal("show");
})

// ── Solicitar Factura ─────────────────────────────────────────────────────
$("#tbventa tbody").on("click", ".btn-factura", function () {
    const venta = $(this).data("venta");
    $("#txtIdVentaFactura").val(venta.idVenta);
    $("#txtNumVentaFactura").val(venta.numeroVenta);
    $("#txtCPFactura").val("");
    $("#cboFormaPago").prop("disabled", false);
    $("#modalFactura").modal("show");
})

// ── Cargar Catálogos ──────────────────────────────────────────────────────
function cargarUsoCFDI() {
    fetch("/Venta/ListaUsoCFDI").then(r => r.json()).then(data => {
        const sel = $("#cboUsoCFDI");
        sel.empty().append('<option value="">-- Seleccione --</option>');
        data.forEach(x => sel.append(`<option value="${x.idUsoCFDI}">${x.cUsoCFDI} - ${x.descripcion}</option>`));
    });
}

function cargarRegimenFiscal() {
    fetch("/Venta/ListaRegimenFiscal").then(r => r.json()).then(data => {
        const sel = $("#cboRegimenFiscalFactura");
        sel.empty().append('<option value="">-- Seleccione --</option>');
        data.forEach(x => sel.append(`<option value="${x.idRegimenFiscal}">${x.cRegimenFiscal} - ${x.descripcion}</option>`));
    });
}

function cargarFormaPago() {
    fetch("/Venta/ListaFormaPago").then(r => r.json()).then(data => {
        const sel = $("#cboFormaPago");
        sel.empty().append('<option value="">-- Seleccione --</option>');
        data.forEach(x => sel.append(`<option value="${x.idFormaPago}">${x.cFormaPago} - ${x.descripcion}</option>`));
    });
}

function cargarMetodoPago() {
    fetch("/Venta/ListaMetodoPago").then(r => r.json()).then(data => {
        const sel = $("#cboMetodoPago");
        sel.empty().append('<option value="">-- Seleccione --</option>');
        data.forEach(x => sel.append(`<option value="${x.idMetodoPago}">${x.cMetodoPago} - ${x.descripcion}</option>`));
    });
}

function cargarTipoComprobante() {
    fetch("/Venta/ListaTipoComprobante").then(r => r.json()).then(data => {
        const sel = $("#cboTipoComprobante");
        sel.empty().append('<option value="">-- Seleccione --</option>');
        data.forEach(x => sel.append(
            `<option value="${x.idTipoDeComprobante}">${x.cTipoDeComprobante} - ${x.descripcion}</option>`
        ));
    });
}