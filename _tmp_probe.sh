#!/bin/bash
M="D:/idocNet/2023.8A.SkyCS/12.Dev.Common/idn.SkyCS.Common/Models"
for f in Mst_VATRate Mst_Brand Mst_Fund Mst_Dealer Mst_GovTaxID Mst_NNTType Mst_CustomerNNTType Mst_VoucherID Mst_ImagePrivateConfig Mst_MapPartColor Mst_CustomerDefaultTiket; do
  echo "===== $f ====="
  cat "$M/$f.cs" 2>/dev/null | head -55
done
