let tablaData;

$(document).ready(function () {

    tablaData = $('#tbdata').DataTable({
        responsive: true,

        data: [
            {
                nombre: "Cliente Demo",
                correo: "cliente@gmail.com",
                rfc: "XAXX010101000",
                cp: "64000",
                regimenFiscal: "General de Ley Personas Morales",
                estado: 1
            }
        ],

        columns: [
            { data: "nombre" },
            { data: "correo" },
            { data: "rfc" },
            { data: "cp" },
            { data: "regimenFiscal" },

            {
                data: "estado",
                render: function (data) {

                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">No Activo</span>';
                }
            },

            {
                defaultContent:
                    '<button class="btn btn-primary btn-sm mr-2"><i class="fas fa-pencil-alt"></i></button>' +
                    '<button class="btn btn-danger btn-sm"><i class="fas fa-trash-alt"></i></button>',

                orderable: false,
                searchable: false,
                width: "80px"
            }
        ],

        dom: "Bfrtip",

        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Clientes'
            },
            'pageLength'
        ],

        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        }

    });

});

$("#btnNuevo").click(function () {

    $("#modalData").modal("show");

});