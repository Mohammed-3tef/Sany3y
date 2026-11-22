"use client";

import React, { useEffect, useState } from "react";
import { apiFetch } from "@/lib/api";
import { City, Governorate } from "@/types";

export function useGovernoratesAndCities(initialGovNameOrId?: string) {
  const [governorates, setGovernorates] = useState<Governorate[]>([]);
  const [cities, setCities] = useState<City[]>([]);
  const [loadingCities, setLoadingCities] = useState(false);

  useEffect(() => {
    apiFetch<Governorate[]>("/api/CountryServices/GetAllGovernorates")
      .then((data) => {
        const sorted = (data || []).sort((a, b) =>
          (a.arabicName || "").localeCompare(b.arabicName || "", "ar")
        );
        setGovernorates(sorted);
      })
      .catch(() => setGovernorates([]));
  }, []);

  const loadCitiesForGovernorate = async (govIdOrName: string) => {
    if (!govIdOrName) {
      setCities([]);
      return;
    }

    let govId = Number(govIdOrName);
    if (isNaN(govId)) {
      const matched = governorates.find(
        (g) => g.arabicName?.trim() === govIdOrName.trim()
      );
      if (matched) govId = matched.id;
    }

    if (!govId || isNaN(govId)) {
      setCities([]);
      return;
    }

    setLoadingCities(true);
    try {
      const data = await apiFetch<City[]>(
        `/api/CountryServices/GetCitiesByGovernorateId/${govId}`
      );
      setCities(data || []);
    } catch {
      setCities([]);
    } finally {
      setLoadingCities(false);
    }
  };

  useEffect(() => {
    if (initialGovNameOrId && governorates.length > 0) {
      loadCitiesForGovernorate(initialGovNameOrId);
    }
  }, [initialGovNameOrId, governorates.length]);

  return {
    governorates,
    cities,
    loadingCities,
    loadCitiesForGovernorate,
  };
}
