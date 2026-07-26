-- BM-PHASE-004.3: CHIFA-OFFICINE schema clone for BM Pharma test
-- Source: BM_PHARMA_CHIFA_DATABASE_CONTRACT.md
-- NOTE: This is a TEST schema based on the documented contract.
--       The REAL CHIFA schema may differ. Validate against the real DB.

-- ============================================================
-- 1. parametre (single-row configuration table)
-- ============================================================
CREATE TABLE parametre (
    code_ps         VARCHAR(10),
    code_centre     VARCHAR(5),
    nom_pharmacie   VARCHAR(50),
    next_num_fact   INTEGER,
    next_num_bord   SMALLINT,
    adresse         VARCHAR(100),
    tel             VARCHAR(20),
    fax             VARCHAR(20),
    email           VARCHAR(50),
    wilaya          VARCHAR(50),
    commune         VARCHAR(50),
    code_postal     VARCHAR(10),
    date_creation   TIMESTAMP,
    date_modification TIMESTAMP,
    version         INTEGER
);

INSERT INTO parametre (code_ps, code_centre, nom_pharmacie, next_num_fact, next_num_bord)
VALUES ('PHARM01', '11600', 'PHARMACIE TEST', 1, 216);

-- ============================================================
-- 2. bordereau (batch envelope)
-- ============================================================
CREATE TABLE bordereau (
    id_bord         BIGSERIAL PRIMARY KEY,
    num_bord        VARCHAR(6) NOT NULL,
    code_centre     VARCHAR(5) NOT NULL,
    etat            CHAR(1),
    id_user_cloture INTEGER,
    poste_cloture   VARCHAR(100),
    mont_vir        NUMERIC(10,2) DEFAULT 0,
    duplicata       BOOLEAN DEFAULT FALSE,
    date_cloture    TIMESTAMP DEFAULT '1900-01-01',
    date_ouverture  TIMESTAMP,
    date_depot_ftp  TIMESTAMP DEFAULT '1900-01-01'
);

CREATE UNIQUE INDEX idx_bordereau_num_bord ON bordereau(num_bord);

-- ============================================================
-- 3. facture (invoice header)
-- ============================================================
CREATE TABLE facture (
    num_fact            VARCHAR(8) PRIMARY KEY,
    date_fact           TIMESTAMP,
    etat                CHAR(1),
    num_bord            VARCHAR(6),
    mont_off            NUMERIC(10,2),
    mont_as             NUMERIC(10,2),
    mont_fact           NUMERIC(11,2),
    num_assure          VARCHAR(12),
    code_centre         VARCHAR(5),
    date_soin           DATE,
    date_fin_droit      DATE,
    date_fin_droit_benef DATE,
    date_envoi_sms      TIMESTAMP,
    date_synchro        TIMESTAMP,
    type_maj            INTEGER NOT NULL DEFAULT 0,
    mont_maj_fae        NUMERIC(4,2) NOT NULL DEFAULT 0,
    mont_maj            NUMERIC(11,2) NOT NULL DEFAULT 0,
    nat_remb            VARCHAR(1) DEFAULT '0',
    mont_mut            NUMERIC(10,2) DEFAULT 0,
    date_fin_mut        DATE DEFAULT '1900-01-01',
    version             VARCHAR(10) DEFAULT '2.0.0',
    num_serie_ps        BIGINT DEFAULT -1,
    version_carte       INTEGER DEFAULT 1,
    echifa              BOOLEAN DEFAULT FALSE,
    id_fact_echifa      BIGINT DEFAULT -1,
    e_ord               BOOLEAN DEFAULT FALSE,
    id_e_ord            BIGINT DEFAULT -1,
    taux                NUMERIC(5,2),
    nom_assure          VARCHAR(50),
    prenom_assure       VARCHAR(50),
    nom_benef           VARCHAR(50),
    prenom_benef        VARCHAR(50),
    lieu_naissance      VARCHAR(50),
    date_naissance      DATE,
    wilaya              VARCHAR(50),
    commune             VARCHAR(50),
    adresse             VARCHAR(100),
    code_postal         VARCHAR(10),
    tel                 VARCHAR(20),
    num_dossier         VARCHAR(20),
    motif_rejet         VARCHAR(200),
    date_rejet          TIMESTAMP,
    date_paiement       TIMESTAMP,
    mont_paiement       NUMERIC(10,2),
    num_cheque          VARCHAR(20),
    date_controle       TIMESTAMP,
    id_utilisateur      INTEGER,
    date_creation       TIMESTAMP,
    date_modification   TIMESTAMP,
    centre_gestion      VARCHAR(10),
    code_acte           VARCHAR(10),
    beneficiaire        VARCHAR(100),
    matricule           VARCHAR(20)
);

CREATE INDEX idx_facture_num_bord ON facture(num_bord);
CREATE INDEX idx_facture_num_assure ON facture(num_assure);
CREATE INDEX idx_facture_date_fact ON facture(date_fact);

-- ============================================================
-- 4. detail_fact (invoice line items)
-- ============================================================
CREATE TABLE detail_fact (
    num_fact        VARCHAR(8) NOT NULL,
    num_enr         VARCHAR(5) NOT NULL,
    ppa             NUMERIC(10,2) NOT NULL,
    qte             NUMERIC(3,0) NOT NULL,
    mont            NUMERIC(10,2) NOT NULL,
    mont_as         NUMERIC(10,2),
    mont_pharm      NUMERIC(10,2),
    num_enr_prescrit VARCHAR(5) NOT NULL,
    num_lot         VARCHAR(6),
    maj_local       NUMERIC(10,2) DEFAULT 0,
    maj_sub         NUMERIC(3,0) DEFAULT 0,
    duree_trait     NUMERIC(3,0) DEFAULT 5,
    tarif_ref       NUMERIC(10,2),
    posologie       VARCHAR(50),
    remboursable    BOOLEAN,
    local           BOOLEAN,
    inf_tr          BOOLEAN DEFAULT TRUE,
    applic_tr       BOOLEAN DEFAULT TRUE,
    medic           BOOLEAN DEFAULT TRUE,
    ts              BOOLEAN DEFAULT TRUE,
    PRIMARY KEY (num_fact, num_enr, ppa)
);

CREATE INDEX idx_detail_fact_num_fact ON detail_fact(num_fact);

-- ============================================================
-- FK: facture.num_bord -> bordereau.num_bord
-- ============================================================
ALTER TABLE facture
    ADD CONSTRAINT fk_facture_bordereau
    FOREIGN KEY (num_bord) REFERENCES bordereau(num_bord);

-- ============================================================
-- FK: detail_fact.num_fact -> facture.num_fact
-- ============================================================
ALTER TABLE detail_fact
    ADD CONSTRAINT fk_detail_fact_facture
    FOREIGN KEY (num_fact) REFERENCES facture(num_fact);
