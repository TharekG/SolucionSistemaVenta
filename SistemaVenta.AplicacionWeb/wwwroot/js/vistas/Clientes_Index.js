const MODELO_BASE = {
    idCliente: 0,
    nombreCliente: "",
    rfcCliente: "",
    direccionFiscal: "",
    idCodigoPostal: null,
    correoElectronico: "",
    idRegimenFiscal: null,
    esActivo: 1,
}

let tablaData;

$(document).ready(function () {

    // Cargar catálogo Régimen Fiscal
    fetch("/Negocio/ListaRegimenFiscal")
        .then(r => r.json())
        .then(data => {
            const sel = $("#cboRegimenFiscal");
            sel.empty().append('<option value="">-- Seleccione --</option>');
            data.forEach(x => sel.append(
                `<option value="${x.idRegimenFiscal}">${x.descripcion}</option>`
            ));
        });

    tablaData = $('#tbdata').DataTable({
        responsive: true,
        "ajax": {
            "url": '/Clientes/Lista',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "idCliente", "visible": false, "searchable": false },
            { "data": "nombreCliente" },
            { "data": "correoElectronico" },
            { "data": "rfcCliente" },
            { "data": "idCodigoPostal" },
            { "data": "descripcionRegimenFiscal" },
            {
                "data": "esActivo", render: function (data) {
                    return data == 1
                        ? '<span class="badge badge-info">Activo</span>'
                        : '<span class="badge badge-danger">No Activo</span>';
                }
            },
            {
                "defaultContent":
                    '<button class="btn btn-primary btn-editar btn-sm mr-2"><i class="fas fa-pencil-alt"></i></button>' +
                    '<button class="btn btn-danger btn-eliminar btn-sm"><i class="fas fa-trash-alt"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Clientes',
                exportOptions: { columns: [1, 2, 3, 4, 5, 6] }
            }, 'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        }
    });
});

function mostrarModal(modelo = MODELO_BASE) {
    $("#txtId").val(modelo.idCliente);
    $("#txtNombreCliente").val(modelo.nombreCliente);
    $("#txtCorreoElectronico").val(modelo.correoElectronico);
    $("#txtRfcCliente").val(modelo.rfcCliente);
    $("#txtDomicilioCP").val(modelo.idCodigoPostal || "");
    $("#cboRegimenFiscal").val(modelo.idRegimenFiscal);
    $("#cboEstado").val(modelo.esActivo);
    $("#modalData").modal("show");
}

$("#btnNuevo").click(function () {
    mostrarModal();
});

$("#btnGuardar").click(function () {

    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter(item => item.value.trim() == "");

    if (inputs_sin_valor.length > 0) {
        toastr.warning("", `Debe completar el campo: "${inputs_sin_valor[0].name}"`);
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus();
        return;
    }

    const modelo = structuredClone(MODELO_BASE);
    modelo["idCliente"] = parseInt($("#txtId").val());
    modelo["nombreCliente"] = $("#txtNombreCliente").val();
    modelo["correoElectronico"] = $("#txtCorreoElectronico").val();
    modelo["rfcCliente"] = $("#txtRfcCliente").val().toUpperCase();
    modelo["idCodigoPostal"] = parseInt($("#txtDomicilioCP").val()) || null;
    modelo["idRegimenFiscal"] = parseInt($("#cboRegimenFiscal").val()) || null;
    modelo["esActivo"] = $("#cboEstado").val();

    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    const url = modelo.idCliente == 0 ? "/Clientes/Crear" : "/Clientes/Editar";
    const method = modelo.idCliente == 0 ? "POST" : "PUT";

    fetch(url, {
        method: method,
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(modelo)
    })
        .then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado) {
                if (modelo.idCliente == 0) {
                    tablaData.row.add(responseJson.objeto).draw(false);
                    swal("¡Listo!", "El cliente fue creado.", "success");
                } else {
                    tablaData.row(filaSeleccionada).data(responseJson.objeto).draw(false);
                    filaSeleccionada = null;
                    swal("¡Listo!", "El cliente fue modificado.", "success");
                }
                $("#modalData").modal("hide");
            } else {
                swal("Lo sentimos", responseJson.mensaje, "error");
            }
        });
});

let filaSeleccionada;

$("#tbdata tbody").on("click", ".btn-editar", function () {
    filaSeleccionada = $(this).closest("tr").hasClass("child")
        ? $(this).closest("tr").prev()
        : $(this).closest("tr");
    mostrarModal(tablaData.row(filaSeleccionada).data());
});

$("#tbdata tbody").on("click", ".btn-eliminar", function () {
    const fila = $(this).closest("tr").hasClass("child")
        ? $(this).closest("tr").prev()
        : $(this).closest("tr");
    const data = tablaData.row(fila).data();

    swal({
        title: "¿Estás seguro?",
        text: `Eliminar al cliente "${data.nombreCliente}"`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Sí, eliminar",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    }, function (respuesta) {
        if (respuesta) {
            $(".showSweetAlert").LoadingOverlay("show");
            fetch(`/Clientes/Eliminar?IdCliente=${data.idCliente}`, { method: "DELETE" })
                .then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })
                .then(responseJson => {
                    if (responseJson.estado) {
                        tablaData.row(fila).remove().draw();
                        swal("¡Listo!", "El cliente fue eliminado.", "success");
                    } else {
                        swal("Lo sentimos", responseJson.mensaje, "error");
                    }
                });
        }
    });
});
