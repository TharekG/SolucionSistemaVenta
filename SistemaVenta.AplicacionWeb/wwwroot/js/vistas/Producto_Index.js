const MODELO_BASE = {
    idProducto: 0,
    codigoBarra: "",
    idMarca: 0,
    modelo: "",
    descripcion: "",
    idCategoria: 0,
    stock: 0,
    urlImagen: "",
    nombreImagen: "",
    precio: 0,
    preciocompra: 0,
    precioventa: 0,
    descuento: 0,
    esActivo: true,
    idMedidaLocal: null,
    idClaveUnidad: null,
    idClaveProdServ: null,
    idObjetoImpuesto: null,
    idImpuesto: null,
    idTipoFactor: null,
    impuestoproducto: 0
}

let tablaData;
let filaSeleccionada;

$(document).ready(function () {

    cargarCategorias();
    cargarMarcas();
    cargarMedidaLocal();
    cargarClaveUnidad();
    cargarObjetoImpuesto();
    cargarImpuesto();
    cargarTipoFactor();
    cargarClaveProdServ();

    tablaData = $('#tbdata').DataTable({
        responsive: true,
        "ajax": {
            "url": '/Producto/Lista',
            "type": "GET",
            "datatype": "json",
            "dataSrc": "data"
        },
        "columns": [
            { "data": "idProducto", "visible": false, "searchable": false },
            { "data": "nombreImagen", "visible": false, "searchable": false },
            { "data": "urlImagen", "visible": false, "searchable": false },
            {
                "data": "urlImagen", render: function (data) {
                    return `<img style="height:60px" src="${data || '/img/noimage.png'}" class="rounded mx-auto d-block"/>`;
                }
            },
            { "data": "codigoBarra" },
            { "data": "descripcionMarca" },
            { "data": "descripcion" },
            { "data": "descripcionCategoria" },
            { "data": "stock" },
            {
                "data": "precioventa", render: function (data) {
                    return data ? `$${parseFloat(data).toFixed(2)}` : "-";
                }
            },
            {
                "data": "descuento", render: function (data) {
                    return data ? `${(parseFloat(data) * 100).toFixed(2)}%` : "0%";
                }
            },
            {
                "data": "esActivo", render: function (data) {
                    if (data)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">No Activo</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-editar btn-sm mr-2"><i class="fas fa-pencil-alt"></i></button>' +
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
                filename: 'Reporte Productos',
                exportOptions: { columns: [4, 5, 6, 7, 8, 9, 10] }
            }, 'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        }
    });

    // ── Botón Nuevo ────────────────────────────────────────────────────────
    $("#btnNuevo").click(function () {
        mostrarModal();
    });

    // ── Guardar ────────────────────────────────────────────────────────────
    $("#btnGuardar").click(function () {

        const inputs = $("input.input-validar").serializeArray();
        const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "");
        if (inputs_sin_valor.length > 0) {
            const mensaje = `Debe completar el campo: "${inputs_sin_valor[0].name}"`;
            toastr.warning("", mensaje);
            $(`input[name="${inputs_sin_valor[0].name}"]`).focus();
            return;
        }

        const descuentoPct = parseFloat($("#txtDescuentoPct").val()) || 0;
        const impuestoPct = parseFloat($("#txtImpuestoPct").val()) || 0;

        const modelo = structuredClone(MODELO_BASE);
        modelo["idProducto"] = parseInt($("#txtId").val());
        modelo["codigoBarra"] = $("#txtCodigoBarra").val();
        modelo["idMarca"] = parseInt($("#cboMarca").val()) || null;
        modelo["modelo"] = $("#txtModelo").val();
        modelo["descripcion"] = $("#txtDescripcion").val();
        modelo["idCategoria"] = parseInt($("#cboCategoria").val()) || null;
        modelo["stock"] = parseInt($("#txtStock").val()) || 0;
        modelo["precioventa"] = parseFloat($("#txtPrecioVenta").val()) || null;
        modelo["precio"] = modelo["precioventa"];
        modelo["preciocompra"] = null;
        modelo["descuento"] = parseFloat((descuentoPct / 100).toFixed(6));
        modelo["esActivo"] = $("#cboEstado").val() === "1";
        modelo["idMedidaLocal"] = parseInt($("#cboMedidaLocal").val()) || null;
        modelo["idClaveUnidad"] = parseInt($("#cboClaveUnidad").val()) || null;
        modelo["idClaveProdServ"] = parseInt($("#cboClaveProdServ").val()) || null;
        modelo["idObjetoImpuesto"] = parseInt($("#cboObjetoImpuesto").val()) || null;
        modelo["idImpuesto"] = parseInt($("#cboImpuesto").val()) || null;
        modelo["idTipoFactor"] = parseInt($("#cboTipoFactor").val()) || null;
        modelo["impuestoproducto"] = parseFloat((impuestoPct / 100).toFixed(6));
        modelo["nombreImagen"] = $("#txtNombreImagen").val();

        const formData = new FormData();
        formData.append("imagen", document.getElementById("txtImagen").files[0]);
        formData.append("modelo", JSON.stringify(modelo));

        $("#modalData").find("div.modal-content").LoadingOverlay("show");

        if (modelo.idProducto == 0) {
            fetch("/Producto/Crear", { method: "POST", body: formData })
                .then(response => {
                    $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })
                .then(responseJson => {
                    if (responseJson.estado) {
                        tablaData.row.add(responseJson.objeto).draw(false);
                        $("#modalData").modal("hide");
                        swal("Listo!", "El producto fue creado", "success");
                    } else {
                        swal("Lo sentimos", responseJson.mensaje, "error");
                    }
                });
        } else {
            fetch("/Producto/Editar", { method: "PUT", body: formData })
                .then(response => {
                    $("#modalData").find("div.modal-content").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })
                .then(responseJson => {
                    if (responseJson.estado) {
                        tablaData.row(filaSeleccionada).data(responseJson.objeto).draw(false);
                        filaSeleccionada = null;
                        $("#modalData").modal("hide");
                        swal("Listo!", "El producto fue modificado", "success");
                    } else {
                        swal("Lo sentimos", responseJson.mensaje, "error");
                    }
                });
        }
    });

    // ── Preview imagen ─────────────────────────────────────────────────────
    $("#txtImagen").change(function () {
        const file = this.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = e => $("#imgProducto").attr("src", e.target.result);
            reader.readAsDataURL(file);
        }
    });

    // ── Botón Editar (event delegation) ───────────────────────────────────
    $("#tbdata tbody").on("click", ".btn-editar", function () {
        if ($(this).closest("tr").hasClass("child")) {
            filaSeleccionada = $(this).closest("tr").prev();
        } else {
            filaSeleccionada = $(this).closest("tr");
        }
        const data = tablaData.row(filaSeleccionada).data();
        mostrarModal(data);
    });

    // ── Botón Eliminar (event delegation) ─────────────────────────────────
    $("#tbdata tbody").on("click", ".btn-eliminar", function () {
        let fila;
        if ($(this).closest("tr").hasClass("child")) {
            fila = $(this).closest("tr").prev();
        } else {
            fila = $(this).closest("tr");
        }
        const data = tablaData.row(fila).data();

        swal({
            title: "Estas seguro?",
            text: `Eliminar el producto "${data.descripcion}"`,
            type: "warning",
            showCancelButton: true,
            confirmButtonClass: "btn-danger",
            cancelButtonText: "No, cancelar",
            closeOnConfirm: false,
            closeOnCancel: true
        },
            function (respuesta) {
                if (respuesta) {
                    $(".showSweetAlert").LoadingOverlay("show");
                    fetch(`/Producto/Eliminar?idProducto=${data.idProducto}`, { method: "DELETE" })
                        .then(response => {
                            $(".showSweetAlert").LoadingOverlay("hide");
                            return response.ok ? response.json() : Promise.reject(response);
                        })
                        .then(responseJson => {
                            if (responseJson.estado) {
                                tablaData.row(fila).remove().draw();
                                swal("Listo!", "El producto fue eliminado", "success");
                            } else {
                                swal("Lo sentimos", responseJson.mensaje, "error");
                            }
                        });
                }
            });
    });
});

// ── Mostrar Modal ─────────────────────────────────────────────────────────
function mostrarModal(modelo = MODELO_BASE) {
    $("#txtId").val(modelo.idProducto);
    $("#txtNombreImagen").val(modelo.nombreImagen || "");
    $("#txtCodigoBarra").val(modelo.codigoBarra);
    $("#cboMarca").val(modelo.idMarca);
    $("#txtModelo").val(modelo.modelo);
    $("#txtDescripcion").val(modelo.descripcion);
    $("#cboCategoria").val(modelo.idCategoria == 0 ? $("#cboCategoria option:first").val() : modelo.idCategoria);
    $("#txtStock").val(modelo.stock);
    $("#txtPrecioVenta").val(modelo.precioventa || modelo.precio);
    $("#txtDescuentoPct").val(modelo.descuento ? (parseFloat(modelo.descuento) * 100).toFixed(2) : "0");
    $("#cboEstado").val(modelo.esActivo ? "1" : "0");
    $("#cboMedidaLocal").val(modelo.idMedidaLocal);
    $("#cboClaveUnidad").val(modelo.idClaveUnidad);
    $("#cboObjetoImpuesto").val(modelo.idObjetoImpuesto);
    $("#cboImpuesto").val(modelo.idImpuesto);
    $("#cboTipoFactor").val(modelo.idTipoFactor);
    $("#txtImpuestoPct").val(modelo.impuestoproducto ? (parseFloat(modelo.impuestoproducto) * 100).toFixed(2) : "0");
    $("#txtImagen").val("");
    $("#imgProducto").attr("src", modelo.urlImagen || "");

    // ClaveProdServ (Select2)
    $("#cboClaveProdServ").val(null).trigger("change");
    if (modelo.idClaveProdServ) {
        const option = new Option(
            modelo.descripcionClaveProdServ || ("Clave: " + modelo.idClaveProdServ),
            modelo.idClaveProdServ, true, true
        );
        $("#cboClaveProdServ").append(option).trigger("change");
    }

    $("#modalData").modal("show");
}

// ── Cargar Catálogos ──────────────────────────────────────────────────────
function cargarCategorias() {
    fetch("/Categoria/Lista")
        .then(r => r.ok ? r.json() : Promise.reject(r))
        .then(responseJson => {
            if (responseJson.data.length > 0) {
                responseJson.data.forEach(item =>
                    $("#cboCategoria").append($("<option>").val(item.idCategoria).text(item.descripcion))
                );
            }
        });
}

function cargarMarcas() {
    fetch("/Producto/ListaMarca")
        .then(r => r.json())
        .then(data => {
            const sel = $("#cboMarca");
            sel.empty().append('<option value="">-- Seleccione --</option>');
            data.forEach(m => sel.append(`<option value="${m.idMarca}">${m.cMarcaCode} - ${m.descripcion}</option>`));
        });
}

function cargarMedidaLocal() {
    fetch("/Producto/ListaMedidaLocal")
        .then(r => r.json())
        .then(data => {
            const sel = $("#cboMedidaLocal");
            sel.empty().append('<option value="">-- Seleccione --</option>');
            data.forEach(m => sel.append(`<option value="${m.idMedidaLocal}">${m.cMedidaLocalCode} - ${m.descripcion}</option>`));
        });
}

function cargarClaveUnidad() {
    fetch("/Producto/ListaClaveUnidad")
        .then(r => r.json())
        .then(data => {
            const sel = $("#cboClaveUnidad");
            sel.empty().append('<option value="">-- Seleccione --</option>');
            data.forEach(u => sel.append(`<option value="${u.idClaveUnidad}">${u.cClaveUnidad} - ${u.nombre}</option>`));
        });
}

function cargarClaveProdServ() {
    $("#cboClaveProdServ").select2({
        dropdownParent: $("#modalData"),
        placeholder: "-- Buscar Clave Prod/Serv --",
        allowClear: true,
        minimumInputLength: 2,
        ajax: {
            url: "/Producto/ListaClaveProdServ",
            dataType: "json",
            delay: 300,
            processResults: function (data) {
                return {
                    results: data.slice(0, 50).map(x => ({
                        id: x.idClaveProdServ,
                        text: `${x.cClaveProdServ} - ${x.descripcion}`
                    }))
                };
            },
            cache: true
        }
    });
}

function cargarObjetoImpuesto() {
    fetch("/Producto/ListaObjetoImpuesto")
        .then(r => r.json())
        .then(data => {
            const sel = $("#cboObjetoImpuesto");
            sel.empty().append('<option value="">-- Seleccione --</option>');
            data.forEach(o => sel.append(`<option value="${o.idObjetoImpuesto}">${o.cObjetoImpuesto} - ${o.descripcion}</option>`));
        });
}

function cargarImpuesto() {
    fetch("/Producto/ListaImpuesto")
        .then(r => r.json())
        .then(data => {
            const sel = $("#cboImpuesto");
            sel.empty().append('<option value="">-- Seleccione --</option>');
            data.forEach(i => sel.append(`<option value="${i.idImpuesto}">${i.cImpuesto} - ${i.descripcion}</option>`));
        });
}

function cargarTipoFactor() {
    fetch("/Producto/ListaTipoFactor")
        .then(r => r.json())
        .then(data => {
            const sel = $("#cboTipoFactor");
            sel.empty().append('<option value="">-- Seleccione --</option>');
            data.forEach(t => sel.append(`<option value="${t.idTipoFactor}">${t.cTipoFactor}</option>`));
        });
}
